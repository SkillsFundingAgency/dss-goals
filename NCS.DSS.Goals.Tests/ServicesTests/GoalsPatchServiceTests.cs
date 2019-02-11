using System;
using DFC.JSON.Standard;
using NCS.DSS.Action.PatchActionsHttpTrigger.Service;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalsHttpTrigger.Service;
using NCS.DSS.Goal.ReferenceData;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
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
        public void GoalPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch { LastModifiedDate = DateTime.MaxValue };

            var updated = _GoalPatchService.Patch(_json, GoalPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var lastModifiedDate = (DateTime)jsonObject["LastModifiedDate"];

            // Assert
            Assert.AreEqual(DateTime.MaxValue, lastModifiedDate);
        }

        [Test]
        public void GoalPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var GoalPatch = new GoalPatch { LastModifiedTouchpointId = "0000000111" };

            var updated = _GoalPatchService.Patch(_json, GoalPatch);

            var jsonObject = (JObject)JsonConvert.DeserializeObject(updated);

            var lastModifiedTouchpointId = jsonObject["LastModifiedTouchpointId"].ToString();

            // Assert
            Assert.AreEqual("0000000111", lastModifiedTouchpointId);
        }

    }
}
