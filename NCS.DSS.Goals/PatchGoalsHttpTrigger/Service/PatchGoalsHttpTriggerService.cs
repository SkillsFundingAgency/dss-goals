using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;
using NCS.DSS.Goals.Models;
using NCS.DSS.Goals.ServiceBus;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public class PatchGoalsHttpTriggerService : IPatchGoalsHttpTriggerService
    {
        public async Task<Models.Goal> UpdateAsync(Models.Goal Goals, GoalPatch GoalsPatch)
        {
            if (Goals == null)
                return null;

            GoalsPatch.SetDefaultValues();

            Goals.Patch(GoalsPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateGoalsAsync(Goals);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? Goals : null;
        }

        public async Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomeId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Goals = await documentDbProvider.GetGoalsForCustomerAsync(customerId, interactionsId, actionplanId, OutcomeId);

            return Goals;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal Goals, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(Goals, customerId, reqUrl);
        }
    }
}