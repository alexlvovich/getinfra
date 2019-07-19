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

        public AzureSBTopicConsumer(ILoggerFactory loggerFactory, IConfiguration configuration, IJsonSerializer serializer, string consumerName)
        {
            _logger = loggerFactory.CreateLogger<AzureSBTopicConsumer>();
            _serializer = serializer;
            _configuration = configuration;

            if (consumerName == null)
            {
                _logger.LogError("consumer name not specified");
                throw new ArgumentNullException("consumer name not specified");
            }

            var consumer = _configuration.GetSection("AzureServiceBus:" + consumerName).Get<ServiceBusConfig>();
            if (consumer == null)
            {
                _logger.LogError("consumer configuration not found");
                throw new Exception("consumer configuration not found");
            }

            var conSting = new ServiceBusConnectionStringBuilder(consumer.Endpoint, consumer.EntityPath, consumer.SasKeyName, consumer.SasKey);

            //var conSting = new ServiceBusConnectionStringBuilder(
            //    _configuration.GetValue<string>("AzureServiceBus:Endpoint"),
            //    _configuration.GetValue<string>("AzureServiceBus:EntityPath"),
            //    _configuration.GetValue<string>("AzureServiceBus:SasKeyName"),
            //    _configuration.GetValue<string>("AzureServiceBus:SasKey"));

            _client = new SubscriptionClient(conSting, consumer.SubscriptionName);
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
