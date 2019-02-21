using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.ServiceBus;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public class PatchGoalHttpTriggerService : IPatchGoalHttpTriggerService
    {

        private readonly IGoalsPatchService _goalPatchService;
        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchGoalHttpTriggerService(IGoalsPatchService goalsPatchService, IDocumentDBProvider documentDbProvider)
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

        public async Task<Models.Goal> UpdateCosmosAsync(string goalJson, Guid goalId)
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

        public async Task SendToServiceBusQueueAsync(Models.Goal Goals, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(Goals, customerId, reqUrl);
        }

    }
}