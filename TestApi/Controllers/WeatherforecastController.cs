using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestApi.Common;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherforecastController : ControllerBase
    {
        private readonly IAmazonSQS _queueClient;
        private readonly QueueSettings _queueSettings;

        public WeatherforecastController(IAmazonSQS queueClinet, 
                               IOptions<QueueSettings> options)
        {            
            _queueClient = queueClinet;

            _queueSettings = options.Value;

            if (_queueSettings.UseLocalDevelopmentSetup)
            {
                var amazonSqsConfig = new AmazonSQSConfig
                {
                    RegionEndpoint = RegionEndpoint.EUWest1,
                    ServiceURL = _queueSettings.LocalServiceUrl
                };

                _queueClient = new AmazonSQSClient(amazonSqsConfig);
            }
        }

        [HttpGet()]
        public async Task<AwsMessage> Get()
        {
            var getQueueMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueSettings.LocalQueueUrl,
                WaitTimeSeconds = _queueSettings.PollTimeInSeconds
            };

            var getMessagesResponse = await _queueClient.ReceiveMessageAsync(getQueueMessageRequest, CancellationToken.None);

            if (getMessagesResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                return new AwsMessage { };
            }

            return new AwsMessage
            {
                Body = getMessagesResponse.Messages[0].Body
            };
        }
    }
}
