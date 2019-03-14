using Newtonsoft.Json;

namespace GetInfra.Standard.Queue.Model
{
    public class QMessage
    {
        public object Body { get; set; }

        [JsonIgnore]
        public QProperties Properties { get; set; }

        [JsonIgnore]
        public ulong DeliveryTag { get; set; }
    }
}
