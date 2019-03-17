using Microsoft.Extensions.Configuration;
using System.IO;

namespace GetInfra.Standard.Ip2Country.Tests
{
    public class BaseServiceTets
    {
        internal IConfiguration _configuration;
        public BaseServiceTets()
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            _configuration = builder.Build();
        }

        
    }
}
