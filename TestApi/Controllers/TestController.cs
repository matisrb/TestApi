using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestApi.Common;
using TestApi.Models;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IAmazonSQS _queueClient;
        private readonly QueueSettings _queueSettings;
        private readonly IAmazonS3 _s3bucketClient;

        public TestController(IAmazonSQS queueClinet,
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

                var s3config = new AmazonS3Config
                {
                    RegionEndpoint = RegionEndpoint.EUWest1,
                    ServiceURL = _queueSettings.LocalServiceUrl,
                    ForcePathStyle = true
                };

                _s3bucketClient = new AmazonS3Client(s3config);
            }
        }

        [HttpGet("my-queue")]
        public async Task<IAwsMessage> Get()
        {
            var getQueueMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueSettings.LocalQueueUrl,
                WaitTimeSeconds = _queueSettings.PollTimeInSeconds
            };

            var getMessagesResponse = await _queueClient.ReceiveMessageAsync(getQueueMessageRequest, CancellationToken.None);

            if (getMessagesResponse.Messages.Any())
            {
                return new AwsMessage
                {
                    Body = getMessagesResponse.Messages[0].Body
                };
            }

            return new EmptyAwsMessage();
        }

        [HttpPut("my-bucket")]
        public async Task PutBucket()
        {
            bool sourceBucketExists = await _s3bucketClient.DoesS3BucketExistAsync("s3://source-bucket");

            var putObjectRequest = new PutObjectRequest
            {
                ContentBody = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor " +
                              "incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco",
                BucketName = "source-bucket",
                Key = "Lorem Ipsum"
            };
            
            var bucketResponse = await _s3bucketClient.PutObjectAsync(putObjectRequest, CancellationToken.None);           
        }

        [HttpGet("my-bucket")]
        public async Task<string> GetBucket()
        {
            bool sourceBucketExists = await _s3bucketClient.DoesS3BucketExistAsync("s3://source-bucket4");

            var getObjectRequest = new GetObjectRequest
            {
                BucketName = "source-bucket",
                Key = "Lorem Ipsum"
            };

            var bucketResponse = await _s3bucketClient.GetObjectAsync(getObjectRequest, CancellationToken.None);

            var content = string.Empty;
            using (var reader = new StreamReader(bucketResponse.ResponseStream))
            {
                content = await reader.ReadToEndAsync();
            }

            return content;
        }
    }
}
