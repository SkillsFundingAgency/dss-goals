using System;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ModelTests
{

    [TestFixture]
    public class GoalsTests
    {

        [Test]
        public void GoalsTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var Goal = new Models.Goal();
            Goal.SetDefaultValues();

            // Assert
            Assert.IsNotNull(Goal.LastModifiedDate);
        }

        [Test]
        public void GoalsTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var Goal = new Models.Goal { LastModifiedDate = DateTime.MaxValue };

            Goal.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, Goal.LastModifiedDate);
        }

        [Test]
        public void GoalsTests_CheckGoalsIdIsSet_WhenSetIdsIsCalled()
        {
            var Goal = new Models.Goal();

            Goal.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, Goal.GoalId);
        }

        [Test]
        public void GoalsTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var Goal = new Models.Goal();

            var customerId = Guid.NewGuid();
            Goal.SetIds(customerId, Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, Goal.CustomerId);
        }

        [Test]
        public void GoalsTests_CheckActionPlanIdIsSet_WhenSetIdsIsCalled()
        {
            var Goal = new Models.Goal();

            var actionPlanId = Guid.NewGuid();
            Goal.SetIds(Arg.Any<Guid>(), actionPlanId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(actionPlanId, Goal.ActionPlanId);
        }

        [Test]
        public void GoalsTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var Goal = new Models.Goal();

            Goal.SetIds(Arg.Any<Guid>(), Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", Goal.LastModifiedTouchpointId);
        }

    }
}
