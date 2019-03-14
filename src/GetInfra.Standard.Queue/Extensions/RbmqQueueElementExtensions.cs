using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json;
using GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Section;

namespace GetInfra.Standard.Queue.Extensions
{

    public static class RbmqQueueElementExtensions
    {
        public static RbmqConfigurationElement ToRbmqConfigurationElement(this RbmqQueueElement q)
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
}
