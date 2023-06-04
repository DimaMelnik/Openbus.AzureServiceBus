using System;
using Openbus.AzureServiceBus.Message;
using Openbus.AzureServiceBus.Transport;

namespace Openbus.AzureServiceBus.UnitTests.Converter
{
    public interface ITestQueue : IBusTopic { }

    public class TestModel : IMessage
    {
        public string Text { get; set; }
        public int Number { get; set; }
        public DateTime Date { get; set; }
    }
}