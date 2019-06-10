using Amazon.DynamoDBv2.DataModel;
using BaseRepositories.DynamoDb.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb.Interfaces
{
    public interface IDynamoDbRepository<T> where T : BaseDynamoDbModel
    {
        Task<T> Add(T model);

        Task AddMany(IEnumerable<T> models);

        Task<bool> CheckHealth();

        Task Delete(string id);

        Task<IEnumerable<T>> GetAll();

        Task<IEnumerable<T>> GetFilter(IEnumerable<ScanCondition> conditions);

        Task<T> GetById(string id);

        Task Update(T model);
    }
}
