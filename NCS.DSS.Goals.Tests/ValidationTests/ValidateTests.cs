﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Goal.ReferenceData;
using NCS.DSS.Goal.Validation;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        private IValidate _validate;

        [SetUp]
        public void Setup()
        {
            _validate = new Validate();
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_DateGoalCapturedMustBeLessThanDateGoalCompletedByAndCurrentDate()
        {
            var goal = new Models.Goal {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-5),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = _validate.ValidateResource(goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_DateGoalAchievedMustBeLessThanCurrentDate()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                DateGoalAchieved = DateTime.Today.AddDays(3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = _validate.ValidateResource(goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_InValidGoalType()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = (GoalType)100,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = _validate.ValidateResource(goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_InValidGoalStatus()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Other,
                GoalStatus= (GoalStatus)1000,
                LastModifiedDate = DateTime.UtcNow
            };

            var result = _validate.ValidateResource(goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedDate = DateTime.Today.AddDays(1)
            };

            var result = _validate.ValidateResource(goal, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedByIsValid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedBy = "0000000001"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedByIsInvalid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedBy = "000000000A"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenGoalSummaryIsValid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedBy = "0000000001"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenGoalSummaryIsInvalid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary[]",
                GoalType = GoalType.Learning,
                LastModifiedBy = "0000000001"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsValid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedBy = "0000000001", 
                SubcontractorId = "123456"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenSubcontractorIdIsInvalid()
        {
            var goal = new Models.Goal
            {
                DateGoalCaptured = DateTime.Today.AddDays(-4),
                DateGoalShouldBeCompletedBy = DateTime.Today.AddDays(-3),
                GoalSummary = "Summary",
                GoalType = GoalType.Learning,
                LastModifiedBy = "0000000001",
                SubcontractorId = "123456X"
            };

            var result = _validate.ValidateResource(goal, false);

            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }


    }
}