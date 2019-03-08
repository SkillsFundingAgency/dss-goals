using System;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ModelTests
{

    [TestFixture]
    public class GoalTests
    {

        [Test]
        public void GoalTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var goal = new Models.Goal();
            goal.SetDefaultValues();

            // Assert
            Assert.IsNotNull(goal.LastModifiedDate);
        }

        [Test]
        public void GoalTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var goal = new Models.Goal { LastModifiedDate = DateTime.MaxValue };

            goal.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.LastModifiedDate);
        }

        [Test]
        public void GoalTests_CheckGoalIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            goal.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, goal.GoalId);
        }

        [Test]
        public void GoalTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            var customerId = Guid.NewGuid();
            goal.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, goal.CustomerId);
        }

        [Test]
        public void GoalTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            var actionPlanId = Guid.NewGuid();
            goal.SetIds(Arg.Any<Guid>(), actionPlanId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(actionPlanId, goal.ActionPlanId);
        }

        [Test]
        public void GoalTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            goal.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", goal.LastModifiedBy);
        }

    }
}
