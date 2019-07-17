using System;
using System.Text;
using System.Threading.Tasks;
using GetInfra.Standard.Queue.Model;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GetInfra.Standard.Queue.Implementations.ServiceBus
{
    public class AzureSBTopicPublisher : IQueuePublisher
    {
        public ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly TopicClient _client;
        private readonly IJsonSerializer _serializer;

        public AzureSBTopicPublisher(ILoggerFactory loggerFactory, IConfiguration configuration, IJsonSerializer serializer, string publisherName)
        {
            _logger = loggerFactory.CreateLogger<AzureSBTopicPublisher>();
            _serializer = serializer;
            _configuration = configuration;

            var publisher = _configuration.GetValue<ServiceBusConfig>("AzureServiceBus:" + publisherName);
            var conSting = new ServiceBusConnectionStringBuilder(publisher.Endpoint, publisher.EntityPath, publisher.SasKeyName, publisher.SasKey);

            //var conSting = new ServiceBusConnectionStringBuilder(
            //    _configuration.GetValue<string>("AzureServiceBus:Endpoint"),
            //    _configuration.GetValue<string>("AzureServiceBus:EntityPath"),
            //    _configuration.GetValue<string>("AzureServiceBus:SasKeyName"),
            //    _configuration.GetValue<string>("AzureServiceBus:SasKey"));

            _client = new TopicClient(conSting);
        }

        public async Task Enqueue(QMessage msg)
        {
            var jsonified = _serializer.Serialize(msg);

            var messageBuffer = Encoding.UTF8.GetBytes(jsonified);
            await _client.SendAsync(new Message(messageBuffer));

        }

       
    }
}
