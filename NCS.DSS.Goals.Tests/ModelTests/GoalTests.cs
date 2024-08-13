using Moq;
using NUnit.Framework;
using System;

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
            Assert.That(goal.LastModifiedDate, Is.Not.Null);
        }

        [Test]
        public void GoalTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var goal = new Models.Goal { LastModifiedDate = DateTime.MaxValue };

            goal.SetDefaultValues();

            // Assert
            Assert.That(goal.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void GoalTests_CheckGoalIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            goal.SetIds(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            Assert.That(goal.GoalId, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void GoalTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            var customerId = Guid.NewGuid();
            goal.SetIds(customerId, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert
            Assert.That(goal.CustomerId, Is.EqualTo(customerId));
        }

        [Test]
        public void GoalTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            var actionPlanId = Guid.NewGuid();
            goal.SetIds(It.IsAny<Guid>(), actionPlanId, It.IsAny<string>(), It.IsAny<string>());

            // Assert
            Assert.That(goal.ActionPlanId, Is.EqualTo(actionPlanId));
        }

        [Test]
        public void GoalTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var goal = new Models.Goal();

            goal.SetIds(It.IsAny<Guid>(), It.IsAny<Guid>(), "0000000000", It.IsAny<string>());

            // Assert
            Assert.That(goal.LastModifiedBy, Is.EqualTo("0000000000"));
        }

    }
}
