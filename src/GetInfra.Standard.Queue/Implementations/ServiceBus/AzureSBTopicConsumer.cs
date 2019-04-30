using System;
using System.Threading;
using System.Threading.Tasks;
using GetInfra.Standard.Queue.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GetInfra.Standard.Queue.Implementations.ServiceBus
{
    public class AzureSBTopicConsumer : IQueueConsumer
    {
        public ILogger _logger;
        private readonly SubscriptionClient _client;
        private readonly IConfiguration _configuration;
        private readonly IJsonSerializer _serializer;
        public AzureSBTopicConsumer(ILoggerFactory loggerFactory, IConfiguration configuration, IJsonSerializer serializer)
        {
            _logger = loggerFactory.CreateLogger<AzureSBTopicConsumer>();
            _serializer = serializer;
            _configuration = configuration;
            var conSting = new ServiceBusConnectionStringBuilder(
                _configuration.GetValue<string>("AzureServiceBus:Endpoint"),
                _configuration.GetValue<string>("AzureServiceBus:EntityPath"),
                _configuration.GetValue<string>("AzureServiceBus:SasKeyName"),
                _configuration.GetValue<string>("AzureServiceBus:SasKey"));
            
            _client = new SubscriptionClient(conSting, _configuration.GetValue<string>("AzureServiceBus:SubscriptionName"));
        }

        public event Action<object, QMessage> MessageRecieved;

        public void Subscribe()
        {
            try
            {
                var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls = 1,
                    AutoComplete = false
                };
                _client.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError("Subscribe error {0}", ex.Message);
            }
        }

        public void Unsubscribe()
        {
            _client.CloseAsync();
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private async Task ReceiveMessagesAsync(Message msg, CancellationToken token)
        {
            var ourMsg = _serializer.Deserialize<QMessage>(msg.Body);
            if (MessageRecieved != null)
                MessageRecieved(_client.SubscriptionName, ourMsg);

            await _client.CompleteAsync(msg.SystemProperties.LockToken);
        }
    }
}
