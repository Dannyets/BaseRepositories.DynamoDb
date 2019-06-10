using Amazon;
using Amazon.Runtime;
using BaseRepositories.DynamoDb.Interfaces;
using BaseRepositories.DynamoDb.Repositories;
using BaseRepositories.DynamoDb.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace BaseRepositories.DynamoDb.Tests
{
    [TestClass]
    public class BaseDynamoDbRepositoryTests
    {
        IDynamoDbRepository<ExerciseDbModel> _repository;

        [TestInitialize]
        public void Init()
        {
            var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY");
            var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY");

            var region = RegionEndpoint.USEast2;
            var credentials = new BasicAWSCredentials(accessKey, secretKey);

            _repository = new BaseDynamoDbRepository<ExerciseDbModel>("Exercise", credentials, region);
        }

        [TestMethod]
        public async Task TestCheckHealth()
        {
            var actualResult = await _repository.CheckHealth();

            Assert.IsTrue(actualResult);
        }
    }
}
