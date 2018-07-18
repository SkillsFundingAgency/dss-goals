using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service
{
    public class GetGoalByIdHttpTriggerService : IGetGoalByIdHttpTriggerService
    {
        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var goal = await documentDbProvider.GetGoalForCustomerAsync(customerId, goalId);

            return goal;
        }
    }
}