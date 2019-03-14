using System;
using System.Collections.Generic;
using System.Text;

namespace GetInfra.Standard.Queue.Implementations.RabbitMq.Config.Json
{
    public class RbmqJsonSection
    {
        public List<RbmqQueueElement> Queues { get; set; }
    }
}
