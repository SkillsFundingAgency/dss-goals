using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.ServiceBus;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Tests.ServicesTests
{
    [TestFixture]
    public class PatchGoalHttpTriggerServiceTests
    {
        private IPatchGoalHttpTriggerService _goalHttpTriggerService;
        private Mock<IGoalPatchService> _goalPatchService;
        private Mock<ICosmosDbProvider> _cosmosDbProvider;
        private Mock<IGoalServiceBusClient> _goalServiceBusClient;
        private Mock<ILogger<PatchGoalHttpTriggerService>> _logger;
        private string _json;
        private Models.Goal _goal;
        private GoalPatch _goalPatch;
        private readonly Guid _goalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _goalPatchService = new Mock<IGoalPatchService>();
            _cosmosDbProvider = new Mock<ICosmosDbProvider>();
            _goalServiceBusClient = new Mock<IGoalServiceBusClient>();
            _logger = new Mock<ILogger<PatchGoalHttpTriggerService>>();
            _goalHttpTriggerService = new PatchGoalHttpTriggerService(_goalPatchService.Object, _cosmosDbProvider.Object, _goalServiceBusClient.Object, _logger.Object);
            _goalPatch = new GoalPatch();
            _goal = new Models.Goal();
            _json = JsonConvert.SerializeObject(_goalPatch);
        }

        [Test]
        public void PatchgoalHttpTriggerServiceTests_PatchResource_ReturnsNullWhenGoalJsonIsNullOrEmpty()
        {
            // Act
            var result = _goalHttpTriggerService.PatchResource(null, It.IsAny<GoalPatch>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void PatchgoalHttpTriggerServiceTests_PatchResource_ReturnsNullWhenGoalPatchIsNullOrEmpty()
        {
            // Act
            var result = _goalHttpTriggerService.PatchResource(It.IsAny<string>(), null);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalIsNullOrEmpty()
        {
            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(null, _goalId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalPatchServicePatchJsonIsNullOrEmpty()
        {
            _goalPatchService.Setup(x => x.Patch(It.IsAny<string>(), It.IsAny<GoalPatch>()));

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(It.IsAny<string>(), _goalId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            var mockItemResponse = new Mock<ItemResponse<Models.Goal>>();

            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.OK);

            _cosmosDbProvider.Setup(x => x.UpdateGoalAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(It.IsAny<string>(), _goalId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            var mockItemResponse = new Mock<ItemResponse<Models.Goal>>();

            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.OK);

            _cosmosDbProvider.Setup(x => x.CreateGoalAsync(It.IsAny<Models.Goal>())).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(_goal.ToString(), _goalId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            // Arrange
            var mockItemResponse = new Mock<ItemResponse<Models.Goal>>();

            var mockGoal = new Models.Goal
            {
                GoalId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                ActionPlanId = Guid.NewGuid()
            };

            mockItemResponse
            .Setup(response => response.Resource)
            .Returns(mockGoal);

            mockItemResponse
            .Setup(response => response.StatusCode)
            .Returns(HttpStatusCode.OK);


            _cosmosDbProvider.Setup(x => x.UpdateGoalAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult(mockItemResponse.Object));

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(_goal.ToString(), _goalId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Goal>());

        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_GetgoalForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _cosmosDbProvider.Setup(x => x.GetGoalForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()));

            // Act
            var result = await _goalHttpTriggerService.GetGoalForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_GetgoalForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _cosmosDbProvider.Setup(x => x.GetGoalForCustomerToUpdateAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(_json));

            // Act
            var result = await _goalHttpTriggerService.GetGoalForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>());

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<string>());
        }
    }
}