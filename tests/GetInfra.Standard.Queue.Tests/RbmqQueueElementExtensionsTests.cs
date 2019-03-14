using GetInfra.Standard.Queue.Extensions;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json;
using Xunit;

namespace Infra.Standard.Queue.Tests
{
    public class RbmqQueueElementExtensionsTests
    {
        [Fact]
        public void ConvertToRbmqConfigurationElement_Test()
        {
            var re = new RbmqQueueElement() {
                AutoDelete = true,
                Bind = false,
                DeadLetters= false,
                Exchange = "TheExchnage",
                ExchangeType = "direct",
                Host = "127.0.0.1",
                IsDurable = true,
                MessageLimit = 5,
                Name = "test",
                Password = "pwd",
                Port = 15678,
                QoS = 1024,
                Queue = "TheQueue",
                RoutingKey ="TheKey",
                Username = "Usr",
                Vhost = "/"
            };


            var e = re.ToRbmqConfigurationElement();

            Assert.Equal(re.AutoDelete, e.AutoDelete);
            Assert.Equal(re.Exchange, e.Exchange);
            Assert.Equal(re.Bind, e.Bind);
            Assert.Equal(re.DeadLetters, e.DeadLetters);
            Assert.Equal(re.ExchangeType, e.ExchangeType);
            Assert.Equal(re.Host, e.Host);
            Assert.Equal(re.IsDurable, e.IsDurable);
            Assert.Equal(re.MessageLimit, e.MessageLimit);
            Assert.Equal(re.Name, e.Name);
            Assert.Equal(re.Password, e.Password);
            Assert.Equal(re.Port, e.Port);
            Assert.Equal(re.QoS, e.QoS);
            Assert.Equal(re.Queue, e.Queue);
            Assert.Equal(re.RoutingKey, e.RoutingKey);
            Assert.Equal(re.Username, e.Username);
            Assert.Equal(re.Vhost, e.Vhost);
        }
    }
}
