using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;
using NCS.DSS.Goals.GetGoalsHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goals.Tests.ServicesTests
{
    [TestFixture]
    public class GetGoalHttpTriggerServiceTests
    {
        private IGetGoalsHttpTriggerService _GoalHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");
        
        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _GoalHttpTriggerService = Substitute.For<GetGoalsHttpTriggerService>(_documentDbProvider);
        }

        [Test]
        public async Task GetGoalHttpTriggerServiceTests_GetGoalsAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetAllGoalsForCustomerAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Goal>>(null).Result);

            // Act
            var result = await _GoalHttpTriggerService.GetGoalsAsync(_customerId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task GetGoalHttpTriggerServiceTests_GetGoalsAsync_ReturnsResource()
        {
            _documentDbProvider.GetAllGoalsForCustomerAsync(Arg.Any<Guid>()).Returns(Task.FromResult(new List<Models.Goal>()).Result);

            // Act
            var result = await _GoalHttpTriggerService.GetGoalsAsync(_customerId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<List<Models.Goal>>(result);
        }
    }
}