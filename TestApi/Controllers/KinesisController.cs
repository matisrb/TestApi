using Amazon.Kinesis;
using Amazon.Kinesis.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class KinesisController : ControllerBase
    {
        private IAmazonKinesis _amazonKinesis;

        public KinesisController(IAmazonKinesis amazonKinesis)
        {
            _amazonKinesis = amazonKinesis;
        }

        [HttpGet("export")]
        public async Task<IActionResult> Get()
        {
            var config = new AmazonKinesisConfig
            {
                ServiceURL = "http://test.localstack:4566"
            };

            _amazonKinesis = new AmazonKinesisClient(config);

            var describeStreamRequest = new DescribeStreamRequest
            {
                StreamName = "testsamplestream"
            };
            var res = await _amazonKinesis.DescribeStreamAsync(describeStreamRequest, CancellationToken.None);

            var streamARN = res.StreamDescription.StreamARN;

            var getRecordsRequest = new GetRecordsRequest
            {
                Limit = 1000
            };

            try
            {
                var records = await _amazonKinesis.GetRecordsAsync(getRecordsRequest, CancellationToken.None);

                //_amazonKinesis.ListStreamConsumersAsync
            }
            catch (Exception exc)
            {
                var ee = exc;
                throw;
            }
            

            return Ok(streamARN);
        }

            [HttpPost("export")]
        public async Task<IActionResult> Post()
        {
            var config = new AmazonKinesisConfig
            {
                ServiceURL = "http://test.localstack:4566"
            };

            _amazonKinesis = new AmazonKinesisClient(config);

            var getRecordsRequest = new GetRecordsRequest
            {
                Limit = 100
            };

            var recordsResponse = await _amazonKinesis.GetRecordsAsync(getRecordsRequest, CancellationToken.None);


            if (recordsResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                var record = recordsResponse.Records.First();
            }

            return Ok();
        }
    }
}
