using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalsByIdHttpTrigger.Service
{
    public class GetGoalsByIdHttpTriggerService : IGetGoalsByIdHttpTriggerService
    {
        public async Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalGuid)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Goals = await documentDbProvider.GetGoalsForCustomerAsync(customerId, goalGuid);

            return Goals;
        }
    }
}