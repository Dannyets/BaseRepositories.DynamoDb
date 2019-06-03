using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using BaseRepositories.Interfaces;
using BaseRepositories.Models;
using BaseRepositories.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb
{
    public class BaseDynamoDbRepository<T> : IBaseRepository<T> where T : BaseDynamoDbModel
    {
        protected readonly string _tableName;

        public BaseDynamoDbRepository(string tableName)
        {
            _tableName = tableName;
        }

        public async Task<T> Add(T dbModel)
        {
            dbModel.Id = new Guid().ToString();
            dbModel.CreatedAt = DateTime.UtcNow;
            dbModel.TransactionStatus = DbTransactionStatus.Pending;

            await Save(dbModel);

            return dbModel;
        }

        public async Task Save(T dbModel)
        {
            await UseContext(async (client, context) =>
            {
                await context.SaveAsync(dbModel);
            });
        }

        public async Task<bool> CheckHealth()
        {
            return await UseContext(IsTableActive);
        }

        public async Task ConfirmTransaction(ConfirmTransactionModel model)
        {
            await UseContext(async (client, context) => 
            {
                await CommitOrDeleteTransaction(client, context, model);
            });
        }

        private async Task<ReturnType> UseContext<ReturnType>(Func<AmazonDynamoDBClient, DynamoDBContext, Task<ReturnType>> asyncFunc)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    return await asyncFunc(client, context);
                }
            }
        }

        private async Task UseContext(Func<AmazonDynamoDBClient, DynamoDBContext, Task> asyncFunc)
        {
            using (var client = new AmazonDynamoDBClient())
            {
                using (var context = new DynamoDBContext(client))
                {
                    await asyncFunc(client, context);
                }
            }
        }

        private async Task CommitOrDeleteTransaction(AmazonDynamoDBClient client, DynamoDBContext context, ConfirmTransactionModel model)
        {
            var record = await context.LoadAsync<T>(model.Id);

            if (record == null)
            {
                throw new KeyNotFoundException($"record with id ${model.Id} not found.");
            }

            if (model.TransactionStatus == DbTransactionStatus.Active)
            {
                record.TransactionStatus = DbTransactionStatus.Active;

                await context.SaveAsync(record);
            }
            else
            {
                await context.DeleteAsync(record);
            }
        }

        private async Task<bool> IsTableActive(AmazonDynamoDBClient client, DynamoDBContext context)
        {
            var tableData = await client.DescribeTableAsync(_tableName);

            return tableData.Table.TableStatus == "active";
        }

    }
}
