using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public class PatchGoalHttpTriggerService : IPatchGoalHttpTriggerService
    {
        public async Task<Models.Goal> UpdateAsync(Models.Goal goal, GoalPatch goalPatch)
        {
            if (goal == null)
                return null;

            if (!goalPatch.LastModifiedDate.HasValue)
                goalPatch.LastModifiedDate = DateTime.Now;

            goal.Patch(goalPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateGoalAsync(goal);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? goal : null;
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var goal = await documentDbProvider.GetGoalForCustomerAsync(customerId, goalId);

            return goal;
        }

    }
}