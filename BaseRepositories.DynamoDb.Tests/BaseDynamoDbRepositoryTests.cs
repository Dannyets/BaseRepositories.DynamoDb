using Amazon;
using Amazon.Runtime;
using BaseRepositories.DynamoDb.Tests.DbTestEntities;
using BaseRepositories.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb.Tests
{
    [TestClass]
    public class BaseDynamoDbRepositoryTests
    {
        IBaseRepository<ExerciseDbModel, string> _repository;

        [TestInitialize]
        public void Init()
        {
            var accessKey = "AKIAJ75UNZD3HLEJVCHQ";
            var secretKey = "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";
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            var region = RegionEndpoint.USEast2;

            _repository = new BaseAwsDynamoDbRepository<ExerciseDbModel>("Exercise", credentials, region);
        }

        [TestMethod]
        public async Task TestCheckHealth()
        {
            var actualResult = await _repository.CheckHealth();

            Assert.IsTrue(actualResult);
        }
    }
}
