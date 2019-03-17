using GetInfra.Standard.Ip2Country.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GetInfra.Standard.Ip2Country
{
    public class IpApiService : IIpLookup
    {
        private readonly IConfiguration _configuration;
        public IpApiService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task<CountryInfo> GetByApiAsync(string ip)
        {
            var c = new CountryInfo();
            var url = string.Format("http://api.ipapi.com/{0}?access_key={1}", ip, _configuration.GetValue<string>("IpApi:ApiKey"));
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(url))
            using (HttpContent content = response.Content)
            {
                // ... Read the string.
                string result = await content.ReadAsStringAsync();

                // Convert input Json string to a dynamic object
                dynamic o = JsonConvert.DeserializeObject(result);

                c.Name = o.country_name;
                c.Code = o.country_code;
            }


            return c;
        }
    }
}
