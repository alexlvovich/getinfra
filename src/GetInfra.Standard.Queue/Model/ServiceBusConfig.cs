using System;
using System.Collections.Generic;
using System.Text;

namespace GetInfra.Standard.Queue.Model
{
    public class ServiceBusConfig
    {
        public string SubscriptionName { get; set; }
        public string Endpoint { get; set; }
        public string EntityPath { get; set; }
        public string SasKeyName { get; set; }
        public string SasKey { get; set; }
    }
}
