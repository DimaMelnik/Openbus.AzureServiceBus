
# Getting Started

Openbus is a straightforward framework that enables you to establish a format for incoming and publishing messages, determine how messages are converted into models, and conduct validation on incoming messages. It offers support for various retry strategies (such as exponential or immediate) when encountering specific exceptions.
 

# Configuration
Configuration section in appsttings.json
### Config Topic
```
"ServiceBusOrder": {
   "ConnectionString": "xxx",
   "Topic": "order",
   "Subscription": "test",
   "PrefetchCount: 5,
   "MaxConcurrentMessagesHandled": 1
}
```
Or with sessions
```
"ServiceBusOrderSessionss": {
   "ConnectionString": "xxx",
   "Topic": "order",
   "Subscription": "test",
    "MaxConcurrentSessionsHandled": 1,
    "MaxConcurrentMessagesHandledPerSession" : 1
}
```

### Config Queue
```
"ServiceBusQueue": {
   "ConnectionString": "xxx",
   "Queue": "test",
   "MaxConcurrentMessagesHandled": 1
}
```
Or with sessions
```
"ServiceBusQueue": {
   "ConnectionString": "xxx",
   "Queue": "test",
   "MaxConcurrentMessagesHandled": 1
}
```

# Topic Bus
In service registration add your transport and messages
```c#
services.AddTopicBus<IMyTopic>(context.Configuration.GetSection("ServiceBusOrder"))
	.WithMessage<TestEvent>(
		c =>
        {

		},
	    c =>
		{
			c.SetExponentialRetryOn<Exception>(10, 40000);
		})
	.AddWoolworthMessageProcessor();
```
or with session
```c#
services.AddTopicBus<IMyTopic>(context.Configuration.GetSection("ServiceBusOrder"))
	.WithMessage<TestEvent,TestEventState>(
		c =>
		{
		},
	    c =>
		{
			c.SetExponentialRetryOn<Exception>(10, 40000);
		})
	.AddWoolworthSessionMessageProcessor();
```

Register your handlers and validators
```c#
    services.AddTransient<IMessageHandler<IMyTopic, TestEvent>, TestEventHandler>();
	services.AddTransient<IMessageValidator<TestEvent>,TestEventValidator>();
```

# Queue Bus
Register your queue transport and processor
```c#
services.AddQueueBus<IMyQueue>(context.Configuration.GetSection("ServiceBusQueue"))
// Register your messages etc
..AddWoolworthMessageProcessor();
```
or with session
```c#
services.AddQueueBus<IMyQueue>(context.Configuration.GetSection("ServiceBusQueue"))
// Register your messages etc
..AddWoolworthSessionMessageProcessor();
```

# Retry
For each message you can register multiple retry strategies for different type of exceptions
```c#
.WithMessage<TestEvent,STestEventState>(
	c =>
	{
	},
	c =>
	{
		c.SetExponentialRetryOn<Exception>(10, 1800, 10);
        c.SetImediateRetryOn<ApiCallException>();
	})
```
Exponential retry in this example will retry 10 times with wait (20,40,80,...,1280,1800,1800,1800)sec between retries.
When deciding what strategy to execute for exception it search for exact match if not found looking for first strategy for the type exception derived from.
For instance previous registration exception on ApiCallException will retry imediate all the rest will retry exponentially.
For Imediate retry it rely on maxDelevery settings of service bus

# Message converters
For each message define converter that describe conversion Model<->ServiceBusTransport
```c#
public class TestEventConverter<TBus> :
        MessageConverter<TBus, TestEvent>
        where TBus: IBus
{
    public override ServiceBusMessage Serialize(TestEvent message)
    {
        var sbMessage = base.Serialize(message);
        //Extended logic

        return sbMessage;
    }
}
```
Register message converter
```c#
services.AddTransient<IMessageConverter<IMyQueue, TestEvent>,TestEventConverter<IMyQueue>>();
```
when registering message you can specify config what should be done in case of failed convertion
```c#
.WithMessage<TestEvent,STestEventState>(
	c =>
	{
        //true is default value which means messages would be completed and not transfered to deadletterqueue
         c.CompleteOnConvertionError = true;
         c.CompleteOnValidationError = true;
	},
```

# Handlers
Implement interface IMessageHandler<IBus,TMessage>
```c#
public class TestEventHandler : IMessageHandler<IMyTopic, TestEvent>
{
    public async Task Handle(TestEvent message, ServiceBusReceivedMessage serviceBusReceivedMessage, CancellationToken cancellationToken)
    {
    }
}
```
or stateful handler
```c#
public class TestStateHandler: IMessageHandler<IMySessionTopic,TestEvent, TestEventState>
{

    public async Task Handle(TestEvent message, TestEventState state, Session.Session session,
    ServiceBusReceivedMessage serviceBusReceivedMessage, CancellationToken cancellationToken)
    { }
}
```
implement interface IOnDeadLetterMessageCallback to receive callback on OnDeadLetterMessage
```c#
 public class TestEventStateHandler : IMessageHandler<IMyTopic, TestEvent,
        TestEventState>, IOnDeadLetterMessageCallback
{
    ...

    public async Task OnDeadLetterMessage(Exception ex, CancellationToken cancellationToken)
    {
            _logger.LogInformation("Dead Letter Message {ex.Message}", ex.Message);
    }
}
```

# Validators
Implement interface IMessageValidator<SendPackingSlipMessage>
```c#
public class TestEventValidator : IMessageValidator<TestEvent>
{
    public Task<ValidationResult> Validate(TestEvent message)
    {
        return ValidationResult.Success();
    }
}
```

# Sender

```c#
private IServiceBusMessageSender<IOrderQueue> serviceBusMessageSenderQueue
...

await _serviceBusMessageSenderQueue.SendAsync(new TestEvent()
                    {
                       ....
                    }, cancelationToken);

```

# Hosted service
Add shoutdown timeout setting to appsettings.json service to allow to porcess all currently processing messages for gracefull shotdown

```
  "HostOptions": {
    "ShutdownTimeout": "00:00:25"
  }
```