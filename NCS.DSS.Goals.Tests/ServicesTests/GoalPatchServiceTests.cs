using System;
using DFC.JSON.Standard;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.ReferenceData;
using Newtonsoft.Json;
using NUnit.Framework;
using Moq;

namespace NCS.DSS.Goal.Tests.ServicesTests
{
    [TestFixture]
    public class GoalPatchServiceTests
    {
        private Mock<IJsonHelper> _jsonHelper;
        private Mock<IGoalPatchService> _goalPatchService;
        private Mock<GoalPatch> _goalPatch;
        private string _json;


        [SetUp]
        public void Setup()
        {
            _jsonHelper = new Mock<IJsonHelper>();
            _goalPatchService = new Mock<IGoalPatchService>(_jsonHelper);
            _goalPatch = new Mock<GoalPatch>();

            _json = JsonConvert.SerializeObject(_goalPatch);
        }

        [Test]
        public void GoalPatchServiceTests_ReturnsNull_WhenGoalPatchIsNull()
        {
            var result = _goalPatchService.Setup(x=> x.Patch(string.Empty, It.IsAny<GoalPatch>()));

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAchievedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch() {  DateGoalAchieved = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Setup(x => x.Patch(_json, goalPatch)).Returns(goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalAchieved);
        }


        [Test]
        public void GoalPatchServiceTests_CheckDateGoalCapturedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch {  DateGoalCaptured = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalCaptured);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalShouldBeCompletedByWhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch {  DateGoalShouldBeCompletedBy = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalShouldBeCompletedBy);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalentToCustomerIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new Models.GoalPatch {  GoalStatus = GoalStatus.Achieved };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(GoalStatus.Achieved, goal.GoalStatus);
        }

        [Test]
        public void GoalPatchServiceTests_CheckGoalSummaryIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch {  GoalSummary  = "Summary" };
            
            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual("Summary", goal.GoalSummary);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAcknowledgedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new Models.GoalPatch {  GoalType = GoalType.Other };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(GoalType.Other, goal.GoalType);
        }


        [Test]
        public void GoalPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch { LastModifiedDate = DateTime.MaxValue };
            
            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.LastModifiedDate);
        }

        [Test]
        public void GoalPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch { LastModifiedBy = "0000000111" };
            
            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);
   
            // Assert
            Assert.AreEqual("0000000111", goal.LastModifiedBy);
        }

    }
}
