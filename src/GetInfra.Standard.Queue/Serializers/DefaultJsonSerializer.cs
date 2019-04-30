using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace GetInfra.Standard.Queue
{
    public class DefaultJsonSerializer : IJsonSerializer
    {
        private JsonSerializerSettings _serializationSettings;

        public DefaultJsonSerializer()
        {
            _serializationSettings = new JsonSerializerSettings()
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
            _serializationSettings.Converters.Add(new StringEnumConverter
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            });
        }

        public string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, _serializationSettings);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value, _serializationSettings);
        }

        public T Deserialize<T>(byte[] data)
        {
            var str = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<T>(str, _serializationSettings);
        }
    }
}
