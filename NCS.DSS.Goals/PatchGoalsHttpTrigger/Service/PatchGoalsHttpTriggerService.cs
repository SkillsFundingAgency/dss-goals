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

        public async Task<Models.Goal> UpdateAsync(string goalJson, GoalPatch goalPatch, Guid goalId)
        {
            if (string.IsNullOrEmpty(goalJson))
                return null;

            if (goalPatch == null)
                return null;

            goalPatch.SetDefaultValues();

            var updatedJson = _goalPatchService.Patch(goalJson, goalPatch);

            if (string.IsNullOrEmpty(updatedJson))
                return null;

            var response = await _documentDbProvider.UpdateGoalsAsync(updatedJson, goalId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? JsonConvert.DeserializeObject<Models.Goal>(updatedJson) : null;
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