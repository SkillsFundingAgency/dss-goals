using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goals.ReferenceData;
using NCS.DSS.Goals.Validation;
using NUnit.Framework;

namespace NCS.DSS.Goals.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {

        [Test]
        public void ValidateTests_ReturnValidationResult_DateGoalCapturedMustBeLessThanDateGoalCompletedByAndCurrentDate()
        {
            var Goal = new Models.Goal {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-5),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.Now
            };

            var validation = new Validate();

            var result = validation.ValidateResource(Goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_DateGoalAchievedMustBeLessThanCurrentDate()
        {
            var Goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                DateGoalAchieved = DateTime.Today.AddDays(3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.Now
            };

            var validation = new Validate();

            var result = validation.ValidateResource(Goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_InValidGoalType()
        {
            var Goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = (GoalType)100,
                LastModifiedDate = DateTime.Now
            };

            var validation = new Validate();

            var result = validation.ValidateResource(Goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_InValidGoalStatus()
        {
            var Goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Other,
                GoalStatus= (GoalStatus)1000,
                LastModifiedDate = DateTime.Now
            };

            var validation = new Validate();

            var result = validation.ValidateResource(Goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var Goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.Today.AddDays(1)
            };
            var validation = new Validate();

            var result = validation.ValidateResource(Goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


    }
}