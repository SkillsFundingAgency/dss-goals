using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.ServiceBus;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.PatchGoalsHttpTrigger.Service
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


        public Models.Goal PatchResource(string goalJson, GoalPatch goalPatch)
        {
            if (string.IsNullOrEmpty(goalJson))
                return null;

            if (goalPatch == null)
                return null;

            goalPatch.SetDefaultValues();

            var updatedgoal = _goalPatchService.Patch(goalJson, goalPatch);

            return updatedgoal;
        }

        public async Task<Models.Goal> UpdateCosmosAsync(Models.Goal goal)
        {
            if (goal == null)
                return null;

            var response = await _documentDbProvider.UpdateGoalsAsync(goal);

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