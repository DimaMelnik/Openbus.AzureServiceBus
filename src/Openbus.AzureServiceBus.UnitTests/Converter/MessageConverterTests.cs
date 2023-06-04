using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.UnitTests.Helper;
using Xunit;

namespace Openbus.AzureServiceBus.UnitTests.Converter
{
    public class MessageConverterTests 
    {
        private Random Random { get; } = new Random();

        public static IEnumerable<object[]> UnconvertibleMessages()
        {
            yield return new object[] { null };
            yield return new object[] { ServiceBusModelFactory.ServiceBusReceivedMessage() };
            yield return new object[] { ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromBytes(new byte[] { })) };
        }

        private ILogger<MessageConverter<ITestQueue, TestModel>> Logger { get; } = Mock.Of<ILogger<MessageConverter<ITestQueue, TestModel>>>();
        private Mock<MessageConverter<ITestQueue, TestModel>> ConverterMock { get; }
        private MessageConverter<ITestQueue, TestModel> Converter => ConverterMock.Object;

        public MessageConverterTests()
        {
            ConverterMock = new Mock<MessageConverter<ITestQueue, TestModel>>(MockBehavior.Default);
            ConverterMock.CallBase = true;
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

    }
}
