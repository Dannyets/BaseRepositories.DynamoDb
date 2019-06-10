using Amazon.DynamoDBv2.DataModel;
using System;

namespace BaseRepositories.DynamoDb.Models
{
    public class BaseDynamoDbModel
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty]
        public DateTime LastUpdatedAt { get; set; }
    }
}
