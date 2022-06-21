using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Kinesis;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.Controllers
{    
    [ApiController]
    [Route("[controller]")]
    public class DynamoDbController : ControllerBase
    {
        private readonly IAmazonDynamoDB _amazonDynamoDB;
        public DynamoDbController(IAmazonDynamoDB amazonDynamoDB)
        {
            _amazonDynamoDB = amazonDynamoDB;
        }

        [HttpPost("music")]
        public async Task<IActionResult> Post(MusicCollection musicCollection)
        {
            var amazonDynamoDBConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://test.localstack:4566"
            };

            var amazonDynamoDBClient = new AmazonDynamoDBClient(amazonDynamoDBConfig);

            var item = new Dictionary<string, AttributeValue>
            {
                { "Artist", new AttributeValue { S = musicCollection.Artist } },
                { "AlbumTitle", new AttributeValue { S = musicCollection.AlbumTitle } },
                { "SongTitle", new AttributeValue { S = musicCollection.SongTitle } }
            };

            var putItemRequest = new PutItemRequest()
            {
                TableName = "MusicCollection",
                Item = item
            };

            await amazonDynamoDBClient.PutItemAsync(putItemRequest);     

            return Created("", JsonConvert.SerializeObject(musicCollection));
        }       

        [HttpGet("{artist}/{song}")]
        public async Task<IActionResult> Get(string artist, string song)
        {
            var amazonDynamoDBConfig = new AmazonDynamoDBConfig
            {
                ServiceURL = "http://test.localstack:4566",
                FastFailRequests = true
            };
            var amazonDynamoDBClient = new AmazonDynamoDBClient(amazonDynamoDBConfig);

            var key = new Dictionary<string, AttributeValue>
            {
                { "Artist", new AttributeValue(s: artist) },
                { "SongTitle", new AttributeValue(s: song) }
            };

            var request = new GetItemRequest
            {
                TableName = "MusicCollection",
                Key = key,
                AttributesToGet = new List<string> { "Artist", "AlbumTitle", "SongTitle" }
            };

            GetItemResponse itemResponse = await amazonDynamoDBClient.GetItemAsync(request);

            var music = new MusicCollection
            {
                Artist = itemResponse.Item["Artist"].S,
                AlbumTitle = itemResponse.Item["AlbumTitle"].S,
                SongTitle = itemResponse.Item["SongTitle"].S
            };

            return Ok(music);
        }

        [HttpPost("populate")]
        public async Task<IActionResult> Generate()
        {
            var amazonDynamoDBConfig = new AmazonDynamoDBConfig();
            amazonDynamoDBConfig.ServiceURL = "http://test.localstack:4566";
            amazonDynamoDBConfig.FastFailRequests = true;
            var amazonDynamoDBClient = new AmazonDynamoDBClient(amazonDynamoDBConfig);

            var writeRequests = new List<WriteRequest>();

            for (int i = 0; i < 15; i++)
            {
                var item = new Dictionary<string, AttributeValue>
                {
                    { "Artist", new AttributeValue { S = $"Artist{i}" } },
                    { "AlbumTitle", new AttributeValue { S = $"AlbumTitle{i}" } },
                    { "SongTitle", new AttributeValue { S = $"SongTitle{i}" } }
                };

                var writeRequest = new WriteRequest
                {
                    PutRequest = new PutRequest
                    {
                        Item = item
                    }
                };

                writeRequests.Add(writeRequest);
            }


            var requestItems = new Dictionary<string, List<WriteRequest>>
            {
                { "MusicCollection", writeRequests }
            };

            var batchWriteRequest = new BatchWriteItemRequest
            {
                RequestItems = requestItems
            };

            var batchWriteResponse = await amazonDynamoDBClient.BatchWriteItemAsync(batchWriteRequest, CancellationToken.None);

            return Ok($"15 Rows added");
        }       
    }
}

