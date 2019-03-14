using Newtonsoft.Json;

namespace GetInfra.Standard.Queue.Model
{
    public class QProperties
    {
        public QProperties()
        {
            Persistent = true;
        }
        
        [JsonIgnore]
        public string UserId { get; set; }
        [JsonIgnore]
        public string ReplyTo { get; set; }
        [JsonIgnore]
        public byte Priority { get; set; }
        [JsonIgnore]
        public string MessageId { get; set; }
        [JsonIgnore]
        public string Expiration { get; set; }
        [JsonIgnore]
        public byte DeliveryMode { get; set; }
        [JsonIgnore]
        public string CorrelationId { get; set; }
        [JsonIgnore]
        public string ContentType { get; set; }
        [JsonIgnore]
        public string ContentEncoding { get; set; }
        [JsonIgnore]
        public string ClusterId { get; set; }
        [JsonIgnore]
        public string AppId { get; set; }
        [JsonIgnore]
        public string Type { get; set; }
        [JsonIgnore]
        public bool Persistent { get; set; }
    }
}
