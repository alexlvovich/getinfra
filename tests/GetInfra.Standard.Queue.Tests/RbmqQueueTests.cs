using System;
using System.Threading;
using Infra.Queueing.Tests.Model;
using Xunit;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging.Abstractions;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json;
using GetInfra.Standard.Queue.Implementations.RabbitMq;
using GetInfra.Standard.Queue;
using GetInfra.Standard.Queue.Model;

namespace Infra.Standard.Queue.Tests
{

    public class RbmqQueueTests
    {
        IConfiguration _configuration;
        public RbmqQueueTests()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("test-settings.json")
                .Build();
        }


        [Fact]
        public void RabbitMq_EnqueueDirectTest()
        {
            var publisherSettings = GetConfig("publisher");
            var consumerSettings = GetConfig("subscriber");
            // check if config exists
            Assert.NotNull(publisherSettings);
            Assert.NotNull(consumerSettings);

            IQueue queue = new RbmqQueue(new NullLoggerFactory(), publisherSettings, consumerSettings);

            queue.Clear();

            var msg = new QMessage();
            var o = new DummyObject() { Id = 1000, Name = "blalbal" };
            msg.Body = o;

            queue.Enqueue(msg);

            var queueuMsg = queue.Dequeue<DummyObject>();

            Assert.NotNull(queueuMsg);

            queue.Cleanup();
        }

        private RbmqConfigurationElement GetConfig(string configName)
        {
            var settings = new RbmqJsonSection();

            _configuration.Bind("rbmqConfigSection", settings);
            // check if config exists
            Assert.NotNull(settings);

            foreach (var q in settings.Queues)
            {
                if (q.Name == configName)
                {
                    return new RbmqConfigurationElement()
                    {
                        AutoDelete = q.AutoDelete,
                        Bind = q.Bind,
                        DeadLetters = q.DeadLetters,
                        Exchange = q.Exchange,
                        ExchangeType = q.ExchangeType,
                        Host = q.Host,
                        IsDurable = q.IsDurable,
                        MessageLimit = q.MessageLimit,
                        Name = q.Name,
                        Password = q.Password,
                        Port = q.Port,
                        QoS = q.QoS,
                        Queue = q.Queue,
                        RoutingKey = q.RoutingKey,
                        Username = q.Username,
                        Vhost = q.Vhost
                    };
                }
            }

            return null;
        }


        //[Test]
        //public void EnqueueFailingTest()
        //{
        //    var failSettings = GetConfig("failureSettings");

        //    // check if config exists
        //    Assert.IsNotNull(failSettings);

        //    var ex = Assert.Throws<Exception>(() => new RbmqQueue(failSettings));

        //    Assert.IsInstanceOf(typeof(Exception), ex);

        //}


        [Fact]
        public void RabbitMq_EnqueueDirectWithProperties()
        {
            var publisherSettings = GetConfig("publisher");
            var consumerSettings = GetConfig("subscriber");
            // check if config exists
            Assert.NotNull(publisherSettings);
            Assert.NotNull(consumerSettings);

            IQueue queue = new RbmqQueue(new NullLoggerFactory(), publisherSettings, consumerSettings);


            queue.Clear();

            var msg = new QMessage();

            msg.Body = new { Id = 1000, Name = "blabla" };

            msg.Properties = new QProperties()
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ReplyTo = "me"
            };

            queue.Enqueue(msg);

            var queueuMsg = queue.Dequeue<DummyObject>();
            Assert.NotNull(queueuMsg);
            Assert.True(msg.Properties.CorrelationId == queueuMsg.Properties.CorrelationId);
            Assert.True(msg.Properties.ReplyTo == queueuMsg.Properties.ReplyTo);

            queue.Cleanup();
        }

        [Fact]
        public void RabbitMq_Subscribe()
        {
            var publisherSettings = GetConfig("publisherFanout");
            var consumerSettings = GetConfig("subscriberFanout");
            // check if config exists
            Assert.NotNull(publisherSettings);
            Assert.NotNull(consumerSettings);

            IQueue cQueue = new RbmqQueue(new NullLoggerFactory(), consumerSettings, false);
            IQueue pQueue = new RbmqQueue(new NullLoggerFactory(), publisherSettings, true);
            cQueue.Clear();
            pQueue.Clear();


            var message = new QMessage()
            {
                Body = "test",
                Properties = new QProperties()
                {
                    ReplyTo = "test"
                }
            };

            pQueue.Enqueue(message);

            QProperties oProperties = null;// = o.Properties;
            string replyTo = string.Empty;
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);
            QMessage recievedMsg = null;
            cQueue.MessageRecieved += (m, o) =>
            {
                cQueue.Unsubscribe();
                recievedMsg = o;
                oProperties = o.Properties;
                replyTo = o.Properties.ReplyTo;
                _autoResetEvent.Set();
            };

            cQueue.Subscribe();


            Assert.True(_autoResetEvent.WaitOne(3000));
            Assert.NotNull(recievedMsg);

            //Assert.That(oProperties, Is.Not.Null);
            //Assert.That(replyTo, Is.EqualTo("test"));

        }


        [Fact]
        public void RabbitMq_EnqueueFanout()
        {
            var publisherSettings = GetConfig("publisherFanout");
            var consumerSettings = GetConfig("subscriberFanout");
            // check if config exists
            Assert.NotNull(publisherSettings);
            Assert.NotNull(consumerSettings);

            IQueue queue = new RbmqQueue(new NullLoggerFactory(), publisherSettings, consumerSettings);

            queue.PublisherSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            queue.ConsumerSerializationSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };

            var msg = new QMessage();
            var o = new DummyObject() { Id = 1000, Name = "blalbal" };
            msg.Body = o;

            queue.Enqueue(msg);


            var a = queue.Dequeue<QMessage>();
            Assert.NotNull(a);
            var body = JsonConvert.DeserializeObject<DummyObject>(a.Body.ToString());
            Assert.True(body.Id == 1000);

            queue.Cleanup();
        }

        [Fact]
        public void RabbitMq_CleanupTest()
        {
            var publisherSettings = GetConfig("publisherFanout");
            var consumerSettings = GetConfig("subscriberFanout");
            // check if config exists
            Assert.NotNull(publisherSettings);
            Assert.NotNull(consumerSettings);

            IQueue queue = new RbmqQueue(new NullLoggerFactory(), publisherSettings, consumerSettings);

            var msg = new QMessage();
            var o = new DummyObject() { Id = 1000, Name = "blalbal" };
            msg.Body = o;
            queue.Clear();
            queue.Enqueue(msg);
            Thread.Sleep(3000);
            var a = queue.Dequeue<QMessage>();
            Assert.NotNull(a);
            var body = JsonConvert.DeserializeObject<DummyObject>(a.Body.ToString());
            Assert.True(body.Id == 1000);
            var ex = Record.Exception(() => queue.Cleanup());
            Assert.Null(ex);

        }

        [Fact]
        public void GetConfig_Test()
        {
            var settings = new MsgQueuesJsonSection();

            _configuration.Bind("MsgQueues", settings);
            // check if config exists
            Assert.NotNull(settings);    
        }


        //[Test]
        //public void RabbitMq_EnqueueFanoutDynamicQueueName()
        //{
        //    var publisherSettings = GetConfig("publisherFanout");
        //    var consumerSettings = GetConfig("subscriberDynamicFanout");
        //    // check if config exists
        //    Assert.IsNotNull(publisherSettings);
        //    Assert.IsNotNull(consumerSettings);

        //    IQueue queue = new RbmqQueue(publisherSettings, consumerSettings);

        //    var msg = new QMessage();
        //    var o = new DummyObject() { Id = 1000, Name = "blalbal" };
        //    msg.Body = o;

        //    queue.Enqueue(msg);


        //    var a = queue.Dequeue<DummyObject>();
        //    Assert.NotNull(a);
        //    var body = (DummyObject)a.Body;
        //    Assert.True(body.Id == 1000);

        //    queue.Cleanup();
        //}
    }
}
