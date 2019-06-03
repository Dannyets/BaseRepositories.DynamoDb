using Amazon.DynamoDBv2.DataModel;
using BaseRepositories.Models;
using BaseRepositories.Models.Enums;
using BaseRepositories.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb
{
    public class BaseDynamoDbModel : IBaseDbModel<string>
    {
        [DynamoDBHashKey]
        public string Id { get; set; }

        [DynamoDBProperty]
        public DateTime CreatedAt { get; set; }

        [DynamoDBProperty]
        public DbTransactionStatus TransactionStatus { get; set; }
    }
}
