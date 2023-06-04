using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.Message
{
    public abstract class GzipCompressedMessageConverter<TBus, TMessage> : IMessageConverter<TBus, TMessage>
        where TMessage : IMessage
        where TBus : IBus
    {
        private readonly ILogger<GzipCompressedMessageConverter<TBus, TMessage>> _logger;
        private const string ContentEncoding = "Content-Encoding";
        private const string GZipEncodingName = "gzip";

        protected GzipCompressedMessageConverter(ILogger<GzipCompressedMessageConverter<TBus, TMessage>> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual JsonSerializerSettings JsonSettings { get; set; } = null;

        public virtual TMessage Deserialize(ServiceBusReceivedMessage receivedMessage)
        {
            var bytes = receivedMessage?.Body?.ToArray();

            //-- sanity check
            if (bytes is not { Length: > 0 })
            {
                return default!;
            }
            
            if (IsCompressed(receivedMessage))
                bytes = UnzipPayload(bytes);

            var messageObject = JsonConvert.DeserializeObject<TMessage>(Encoding.UTF8.GetString(bytes), JsonSettings);
            if (messageObject == null)
            {
                _logger.LogWarning("Deserialization Message Body to {@Type} resulted in a null", typeof(TMessage));
                return default!;
            }

            return messageObject;
        }

        public virtual ServiceBusMessage Serialize(TMessage message)
        {
            var correlationId = Guid.NewGuid().ToString("N");
            var messageToSend = JsonConvert.SerializeObject(message);
            var payloadToSend = Encoding.UTF8.GetBytes(messageToSend);
            
            payloadToSend = ZipPayload(payloadToSend);

            var sbMessage = new ServiceBusMessage(payloadToSend)
            {
                MessageId = Guid.NewGuid().ToString("N"),
                ContentType = "text/json",
                CorrelationId = correlationId,
                ApplicationProperties =
                {
                    {ContentEncoding, GZipEncodingName}
                }
            };
            return sbMessage;
        }

        public abstract bool CanDeserialize(ServiceBusReceivedMessage message);

        #region Private Methods

        private static bool IsCompressed(ServiceBusReceivedMessage message)
        {
            return message.ApplicationProperties.TryGetValue(ContentEncoding, out var enc)
                   && enc is string encoding
                   && string.Equals(encoding, GZipEncodingName, StringComparison.OrdinalIgnoreCase);
        }

        private static byte[] UnzipPayload(byte[] byteArray)
        {
            using var compressed = new MemoryStream(byteArray);
            using var decompressor = new GZipStream(compressed, CompressionMode.Decompress);
            using var decompressed = new MemoryStream();

            decompressor.CopyTo(decompressed);
            decompressed.Flush();
            return decompressed.ToArray(); //-- replace the compressed bytes with the decompressed ones
        }

        private static byte[] ZipPayload(byte[] bytes)
        {
            using var uncompressed = new MemoryStream(bytes);
            using var compressed = new MemoryStream();
            using var compressor = new GZipStream(compressed, CompressionMode.Compress);

            uncompressed.CopyTo(compressor);
            compressor.Flush();
            return compressed.ToArray();
        }
        #endregion
    }
}