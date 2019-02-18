using System;
using DFC.JSON.Standard;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalsHttpTrigger.Service;
using NCS.DSS.Goal.ReferenceData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServiceTests
{
    [TestFixture]
    public class GoalPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private IGoalsPatchService _GoalPatchService;
        private GoalPatch _GoalPatch;
        private string _json;


        [SetUp]
        public void Setup()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _GoalPatchService = Substitute.For<GoalsPatchService>(_jsonHelper);
            _GoalPatch = Substitute.For<GoalPatch>();

            _json = JsonConvert.SerializeObject(_GoalPatch);
        }

        [Test]
        public void GoalPatchServiceTests_ReturnsNull_WhenGoalPatchIsNull()
        {
            var result = _GoalPatchService.Patch(string.Empty, Arg.Any<GoalPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAchievedIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch() {  DateGoalAchieved = DateTime.MaxValue };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var dateGoalCreated = Goal.DateGoalAchieved;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, dateGoalCreated);
        }


        [Test]
        public void GoalPatchServiceTests_CheckDateGoalCapturedIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch {  DateGoalCaptured = DateTime.MaxValue };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var value = Goal.DateGoalCaptured;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, value);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalShouldBeCompletedByWhenPatchIsCalled()
        {
            var GoalPatch = new Models.GoalPatch {  DateGoalShouldBeCompletedBy = DateTime.MaxValue };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var val = Goal.DateGoalShouldBeCompletedBy;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, val);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalSentToCustomerIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new Models.GoalPatch {  GoalStatus = GoalStatus.Achieved };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var val = Goal.GoalStatus;

            // Assert
            Assert.AreEqual(GoalStatus.Achieved, val);
        }

        [Test]
        public void GoalPatchServiceTests_CheckGoalSummaryIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch {  GoalSummary  = "Summary" };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var val = Goal.GoalSummary;

            // Assert
            Assert.AreEqual("Summary", val);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAcknowledgedIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new Models.GoalPatch {  GoalType = GoalType.Other };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var val = Goal.GoalType;

            // Assert
            Assert.AreEqual(GoalType.Other, val);
        }


        [Test]
        public void GoalPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch { LastModifiedDate = DateTime.MaxValue };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var lastModifiedDate = Goal.LastModifiedDate;

            // Assert
            Assert.AreEqual(DateTime.MaxValue, lastModifiedDate);
        }

        [Test]
        public void GoalPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch { LastModifiedTouchpointId = "0000000111" };

            var Goal = _GoalPatchService.Patch(_json, GoalPatch);

            var lastModifiedTouchpointId = Goal.LastModifiedTouchpointId;

            // Assert
            Assert.AreEqual("0000000111", lastModifiedTouchpointId);
        }

    }
}
