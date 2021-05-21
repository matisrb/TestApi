using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestApi.Common
{
    public class QueueSettings
    {
        public bool UseLocalDevelopmentSetup { get; set; }
        public string LocalQueueUrl { get; set; }
        public string LocalServiceUrl { get; set; }
        public string QueueName { get; set; }
        public int PollTimeInSeconds { get; set; }
    }
}
