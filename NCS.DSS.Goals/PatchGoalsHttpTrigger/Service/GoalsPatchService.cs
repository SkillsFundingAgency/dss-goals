using DFC.JSON.Standard;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalsHttpTrigger.Service;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Action.PatchActionsHttpTrigger.Service
{
    public class GoalsPatchService : IGoalsPatchService
    {
        private IJsonHelper _jsonHelper;

        public GoalsPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }
        public string Patch(string goalsJson, GoalPatch goalPatch)
        {
            if (string.IsNullOrEmpty(goalsJson))
                return null;

            var obj = JObject.Parse(goalsJson);

            if (goalPatch.DateGoalCaptured.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateGoalCaptured"], goalPatch.DateGoalCaptured);

            if (goalPatch.DateGoalShouldBeCompletedBy.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateGoalShouldBeCompletedBy"], goalPatch.DateGoalShouldBeCompletedBy);

            if (goalPatch.DateGoalAchieved.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["DateGoalAchieved"], goalPatch.DateGoalAchieved);

            if (!string.IsNullOrEmpty(goalPatch.GoalSummary))
                _jsonHelper.UpdatePropertyValue(obj["GoalSummary"], goalPatch.GoalSummary);

            if (goalPatch.GoalType.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["GoalType"], goalPatch.GoalType);

            if (goalPatch.GoalStatus.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["GoalStatus"], goalPatch.GoalStatus);

            if (goalPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], goalPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(goalPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], goalPatch.LastModifiedTouchpointId);


            return obj.ToString();
        }
    }
}
