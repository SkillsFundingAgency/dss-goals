using System;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;
using NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goals.Tests.ServicesTests
{
    [TestFixture]
    public class GetGoalsByIdHttpTriggerServiceTests
    {
        private IGetGoalsByIdHttpTriggerService _GoalHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private Models.Goal _Goal;
        private readonly Guid _goalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");
        private readonly Guid _customerId = Guid.Parse("58b43e3f-4a50-4900-9c82-a14682ee90fa");


        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _GoalHttpTriggerService = Substitute.For<GetGoalsByIdHttpTriggerService>(_documentDbProvider);
            _Goal = Substitute.For<Models.Goal>();
        }


        [Test]
        public async Task GetGoalHttpTriggerServiceTests_GetGoalForCustomerAsync_ReturnsResource()
        {

            _documentDbProvider.GetGoalsForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_Goal).Result);

            // Act
            var result = await _GoalHttpTriggerService.GetGoalsForCustomerAsync(_customerId, _goalId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Goal>(result);
        }



        [Test]
        public async Task GetGoalHttpTriggerServiceTests_GetGoalForCustomerAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.GetGoalsForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            // Act
            var result = await _GoalHttpTriggerService.GetGoalsForCustomerAsync(_customerId, _goalId);

            // Assert
            Assert.IsNull(result);
        }
    }
}