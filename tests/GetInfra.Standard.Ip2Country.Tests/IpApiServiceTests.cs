using System;
using System.Threading.Tasks;
using Xunit;

namespace GetInfra.Standard.Ip2Country.Tests
{
    public class IpApiServiceTests : BaseServiceTets
    {
        [Fact]
        public async Task GetCountryByIp()
        {
            var lookupService = new IpApiService(_configuration);

            var c = await lookupService.GetByApiAsync("178.9.118.185");

            Assert.NotNull(c);
        }
    }
}
