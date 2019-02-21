using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.ReferenceData;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServiceTests
{
    [TestFixture]
    public class GoalPatchServiceTests
    {
        private IGoalsPatchService _goalPatchService;
        private GoalPatch _goalPatch;
        private string _json;


        [SetUp]
        public void Setup()
        {
            _goalPatchService = Substitute.For<GoalsPatchService>();
            _goalPatch = Substitute.For<GoalPatch>();

            _json = JsonConvert.SerializeObject(_goalPatch);
        }

        [Test]
        public void GoalPatchServiceTests_ReturnsNull_WhenGoalPatchIsNull()
        {
            var result = _goalPatchService.Patch(string.Empty, Arg.Any<GoalPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAchievedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch() { DateGoalAchieved = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalAchieved);
        }


        [Test]
        public void GoalPatchServiceTests_CheckDateGoalCapturedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch { DateGoalCaptured = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalCaptured);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalShouldBeCompletedByWhenPatchIsCalled()
        {
            var goalPatch = new Models.GoalPatch { DateGoalShouldBeCompletedBy = DateTime.MaxValue };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, goal.DateGoalShouldBeCompletedBy);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalSentToCustomerIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new Models.GoalPatch { GoalStatus = GoalStatus.Achieved };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual(GoalStatus.Achieved, goal.GoalStatus);
        }

        [Test]
        public void GoalPatchServiceTests_CheckGoalSummaryIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new GoalPatch { GoalSummary = "Summary" };

            var patchedGoal = _goalPatchService.Patch(_json, goalPatch);

            var goal = JsonConvert.DeserializeObject<Models.Goal>(patchedGoal);

            // Assert
            Assert.AreEqual("Summary", goal.GoalSummary);
        }

        [Test]
        public void GoalPatchServiceTests_CheckDateGoalAcknowledgedIsUpdated_WhenPatchIsCalled()
        {
            var goalPatch = new Models.GoalPatch { GoalType = GoalType.Other };

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