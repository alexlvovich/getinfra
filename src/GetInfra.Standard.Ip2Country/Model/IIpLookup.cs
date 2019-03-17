using GetInfra.Standard.Ip2Country.Model;
using System;
using System.Threading.Tasks;

namespace GetInfra.Standard.Ip2Country.Model
{
    public interface IIpLookup
    {
        Task<CountryInfo> GetByApiAsync(string ip);
    }
}
