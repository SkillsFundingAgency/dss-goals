using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.ServiceBus;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Tests.ServicesTests
{
    [TestFixture]
    public class PostGoalHttpTriggerServiceTests
    {
        private IPostGoalHttpTriggerService _goalHttpTriggerService;
        private Mock<ICosmosDbProvider> _cosmosDbProvider;
        private Mock<IGoalServiceBusClient> _goalServiceBusClient;
        private Mock<ILogger<PostGoalHttpTriggerService>> _logger;
        private string _json;
        private Models.Goal _goal;
        private readonly Guid _goalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _cosmosDbProvider = new Mock<ICosmosDbProvider>();
            _goalServiceBusClient = new Mock<IGoalServiceBusClient>();
            _logger = new Mock<ILogger<PostGoalHttpTriggerService>>();
            _goalHttpTriggerService = new PostGoalHttpTriggerService(_cosmosDbProvider.Object, _goalServiceBusClient.Object, _logger.Object);
            _goal = new Models.Goal();
            _json = JsonConvert.SerializeObject(_goal);
        }

        [Test]
        public async Task PostGoalHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionPlanJsonIsNull()
        {
            // Act
            var result = await _goalHttpTriggerService.CreateAsync(It.IsAny<Models.Goal>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostGoalHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Goal>>();

            var mockGoal = new Models.Goal
            {
                CustomerId = Guid.NewGuid(),
                ActionPlanId = Guid.NewGuid()
            };

            mockItemResponse
            .Setup(response => response.Resource)
            .Returns(mockGoal);

            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.Created);

            _cosmosDbProvider.Setup(x => x.CreateGoalAsync(_goal)).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _goalHttpTriggerService.CreateAsync(_goal);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Goal>());
        }
    }
}