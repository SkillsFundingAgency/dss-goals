using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public class GoalsPatchService : IGoalsPatchService
    {
        public string Patch(string goalsJson, GoalPatch goalPatch)
        {
            if (string.IsNullOrEmpty(goalsJson))
                return null;

            var obj = JObject.Parse(goalsJson);

            if (goalPatch.DateGoalCaptured.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateGoalCaptured"], goalPatch.DateGoalCaptured);

            if (goalPatch.DateGoalShouldBeCompletedBy.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateGoalShouldBeCompletedBy"],
                    goalPatch.DateGoalShouldBeCompletedBy);

            if (goalPatch.DateGoalAchieved.HasValue)
                JsonHelper.UpdatePropertyValue(obj["DateGoalAchieved"], goalPatch.DateGoalAchieved);

            if (!string.IsNullOrEmpty(goalPatch.GoalSummary))
                JsonHelper.UpdatePropertyValue(obj["GoalSummary"], goalPatch.GoalSummary);

            if (goalPatch.GoalType.HasValue)
                JsonHelper.UpdatePropertyValue(obj["GoalType"], goalPatch.GoalType);

            if (goalPatch.GoalStatus.HasValue)
                JsonHelper.UpdatePropertyValue(obj["GoalStatus"], goalPatch.GoalStatus);

            if (goalPatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], goalPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(goalPatch.LastModifiedBy))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedBy"], goalPatch.LastModifiedBy);
            
            return obj.ToString();

        }
    }
}
