using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Moq;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;
using Openbus.AzureServiceBus.UnitTests.Helper;
using Xunit;

namespace Openbus.AzureServiceBus.UnitTests.Converter
{
    public class GzipCompressedMessageConverterTests 
    {
        private Random Random { get; } = new Random();

        public static IEnumerable<object[]> UnconvertibleMessages()
        {
            yield return new object[] { null };
            yield return new object[] { ServiceBusModelFactory.ServiceBusReceivedMessage() };
            yield return new object[] { ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(new byte[] { })) };
        }

        private ILogger<GzipCompressedMessageConverter<ITestQueue, TestModel>> Logger { get; } = Mock.Of<ILogger<GzipCompressedMessageConverter<ITestQueue, TestModel>>>();
        private Mock<GzipCompressedMessageConverter<ITestQueue, TestModel>> ConverterMock { get; }
        private GzipCompressedMessageConverter<ITestQueue, TestModel> Converter => ConverterMock.Object;

        public GzipCompressedMessageConverterTests()
        {
            ConverterMock = new Mock<GzipCompressedMessageConverter<ITestQueue, TestModel>>(MockBehavior.Default, Logger);
            ConverterMock.CallBase = true;
        }

        [Theory]
        [MemberData(nameof(UnconvertibleMessages))]
        public void TryConvert_WhenUnconvertibleMessage_ReturnsFalse(ServiceBusReceivedMessage msg)
        {
            // ARRANGE
            // ACT
            var obj = Converter.Deserialize(msg);

            // ASSERT
            obj.Should().BeNull();
        }


        [Fact]
        public void TryConvert_WhenMessageHasUncompressedData_ReturnsTrueAndDeserialisesObject()
        {
            // ARRANGE
            var original = new TestModel { Text = Random.NextString(15, 20), Number = Random.NextInt32() };
            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(original))));

            // ACT
            var actual = Converter.Deserialize(msg);

            // ASSERT
            actual.Should().BeEquivalentTo(original);
        }

        [Fact]
        public void TryConvert_WhenMessageHasCompressedData_ReturnsTrueAndDeserialisesObject()
        {
            // ARRANGE
            var original = new TestModel { Text = Random.NextString(10, 25), Number = Random.NextInt32() };
            var originalBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(original));
            using var compressed = new MemoryStream();
            using var compressor = new GZipStream(compressed, Random.NextEnum<CompressionLevel>());
            compressor.Write(originalBytes, 0, originalBytes.Length);
            compressor.Flush();

            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(compressed.ToArray()),
                properties: new Dictionary<string, object> { { "Content-Encoding", "gzip" } });

            // ACT
            var actual = Converter.Deserialize(msg);

            // ASSERT
            actual.Should().BeEquivalentTo(original);
        }

        [Theory]
        [InlineData("2020-11-20T11:00:00.0000000+00:00", DateTimeKind.Local)]
        [InlineData("2020-11-20T11:00:00.0000000+13:00", DateTimeKind.Local)]
        [InlineData("2020-11-20T11:00:00.0000000", DateTimeKind.Unspecified)]
        [InlineData("2020-11-20T11:00:00.0000000Z", DateTimeKind.Utc)]
        public void TryConvert_WhenMessageHasDateFields_ReturnsTrueAndUsesRoundTripDateKind(string dateString, DateTimeKind expectedKind)
        {
            // ARRANGE
            var json = "{ \"Date\":\"" + dateString + "\" }";
            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(Encoding.UTF8.GetBytes(json)));

            // ACT
            var actual = Converter.Deserialize(msg);

            // ASSERT
            actual.Should().BeOfType<TestModel>().Which.Date.Kind.Should().Be(expectedKind);
        }

        [Fact]
        public void TryConvert_WhenMessageHasAlotOfCompressedData_ReturnsTrueAndDeserialisesObject()
        {
            // ARRANGE
            var original = new TestModel { Text = Random.NextString(192 * 1024, 512 * 1024), Number = Random.NextInt32() };
            var originalBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(original));
            using var compressed = new MemoryStream();
            using var compressor = new GZipStream(compressed, Random.NextEnum<CompressionLevel>());
            compressor.Write(originalBytes, 0, originalBytes.Length);
            compressor.Flush();

            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(compressed.ToArray()),
                                                                                            properties: new Dictionary<string, object> { { "Content-Encoding", "gzip" } });

            // ACT
            var actual = Converter.Deserialize(msg);

            // ASSERT
            actual.Should().BeEquivalentTo(original);
        }

        [Fact]
        public void GivenTestModel_WhenMessageIsSerialized_ThenDeserializeReturnCorrectTestModel()
        {
            // ARRANGE
            var original = new TestModel { Text = Random.NextString(15, 20), Number = Random.NextInt32() };
            var gzipServiceBusMessage = Converter.Serialize(original);
            var receivedMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(body: gzipServiceBusMessage.Body,
                properties: gzipServiceBusMessage.ApplicationProperties);

            // ACT
            var actual = Converter.Deserialize(receivedMessage);

            // ASSERT
            actual.Should().BeEquivalentTo(original);
        }

    }
}
