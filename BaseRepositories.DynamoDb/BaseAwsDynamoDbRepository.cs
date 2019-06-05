using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;
using BaseRepositories.Interfaces;
using BaseRepositories.Models;
using BaseRepositories.Models.Enums;
using BaseRepositories.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb
{
    public class BaseAwsDynamoDbRepository<T> : IBaseRepository<T, string> where T : IBaseDbModel<string>
    {
        protected readonly string _tableName;
        private readonly AWSCredentials _credentials;
        private readonly RegionEndpoint _regionEndpoint;

        public BaseAwsDynamoDbRepository(string tableName, 
                                         AWSCredentials credentials, 
                                         RegionEndpoint regionEndpoint)
        {
            _tableName = tableName;
            _credentials = credentials;
            _regionEndpoint = regionEndpoint;
        }

        public async Task<T> Add(T dbModel)
        {
            FillNewModelProperties(dbModel);

            await Update(dbModel);

            return dbModel;
        }

        public async Task<bool> CheckHealth()
        {
            return await UseContext(IsTableActive);
        }

        public async Task ConfirmTransaction(ConfirmTransactionModel<string> model)
        {
            await UseContext(async (client, context) => 
            {
                await CommitOrDeleteTransaction(client, context, model);
            });
        }

        public Task<IEnumerable<T>> GetAll()
        {
            throw new NotImplementedException();
        }

        public async Task<T> GetById(string id)
        {
            return await UseContext(async (client, context) =>
            {
                var record = await context.LoadAsync<T>(id);

                return record;
            });
        }

        public async Task Delete(string id)
        {
            await UseContext(async (client, context) =>
            {
                await context.DeleteAsync<T>(id);
            });
        }

        public async Task Update(T model)
        {
            await UseContext(async (client, context) =>
            {
                await context.SaveAsync(model);
            });
        }

        public async Task AddMany(IEnumerable<T> models)
        {
            await UseContext(async (client, context) =>
            {
                var saveToDbTasks = models.Select(m =>
                {
                    FillNewModelProperties(m);

                    return context.SaveAsync(m);
                });

                await Task.WhenAll(saveToDbTasks);
            });
        }

        private async Task<ReturnType> UseContext<ReturnType>(Func<AmazonDynamoDBClient, DynamoDBContext, Task<ReturnType>> asyncFunc)
        {
            using (var client = new AmazonDynamoDBClient(_credentials, _regionEndpoint))
            {
                using (var context = new DynamoDBContext(client))
                {
                    return await asyncFunc(client, context);
                }
            }
        }

        private async Task UseContext(Func<AmazonDynamoDBClient, DynamoDBContext, Task> asyncFunc)
        {
            using (var client = new AmazonDynamoDBClient(_credentials, _regionEndpoint))
            {
                using (var context = new DynamoDBContext(client))
                {
                    await asyncFunc(client, context);
                }
            }
        }

        private async Task CommitOrDeleteTransaction(AmazonDynamoDBClient client, DynamoDBContext context, ConfirmTransactionModel<string> model)
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

            var tableStatus = tableData.Table.TableStatus.Value.ToLower();

            return tableStatus == "active";
        }

        private void FillNewModelProperties(T dbModel)
        {
            dbModel.Id = Guid.NewGuid().ToString();
            dbModel.CreatedAt = DateTime.UtcNow;
            dbModel.TransactionStatus = DbTransactionStatus.Pending;
        }
    }
}
