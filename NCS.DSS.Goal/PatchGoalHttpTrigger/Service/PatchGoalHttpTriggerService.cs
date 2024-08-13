using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.ServiceBus;
using System.Net;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public class PatchGoalHttpTriggerService : IPatchGoalHttpTriggerService
    {

        private readonly IGoalPatchService _goalPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchGoalHttpTriggerService(IGoalPatchService goalPatchService, IDocumentDBProvider documentDbProvider)
        {
            _goalPatchService = goalPatchService;
            _documentDbProvider = documentDbProvider;
        }


        public string PatchResource(string goalJson, GoalPatch goalPatch)
        {
            if (string.IsNullOrEmpty(goalJson))
                return null;

            if (goalPatch == null)
                return null;

            goalPatch.SetDefaultValues();

            var updatedGoal = _goalPatchService.Patch(goalJson, goalPatch);

            return updatedGoal;
        }

        public async Task<Models.Goal> UpdateCosmosAsync(string goalJson, Guid goalId)
        {
            if (string.IsNullOrEmpty(goalJson))
                return null;

            var response = await _documentDbProvider.UpdateGoalAsync(goalJson, goalId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            var goal = await _documentDbProvider.GetGoalForCustomerToUpdateAsync(customerId, goalId, actionPlanId);

            return goal;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal goal, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(goal, customerId, reqUrl);
        }
    }
}