using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goals.Models;
using NCS.DSS.Goals.ServiceBus;
using NCS.DSS.Goals.Cosmos.Provider;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public class PatchGoalsHttpTriggerService : IPatchGoalsHttpTriggerService
    {

        private readonly IGoalsPatchService _goalPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchGoalsHttpTriggerService(IGoalsPatchService goalsPatchService, IDocumentDBProvider documentDbProvider)
        {
            _goalPatchService = goalsPatchService;
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

        public async Task<Goal> UpdateCosmosAsync(string goalJson, Guid goalId)
        {
            if (string.IsNullOrEmpty(goalJson))
                return null;

            var response = await _documentDbProvider.UpdateGoalAsync(goalJson, goalId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId)
        {
            var goal = await _documentDbProvider.GetGoalForCustomerToUpdateAsync(customerId, goalId);

            return goal;
        }

        public async Task SendToServiceBusQueueAsync(Goal goal, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(goal, customerId, reqUrl);
        }
    }
}