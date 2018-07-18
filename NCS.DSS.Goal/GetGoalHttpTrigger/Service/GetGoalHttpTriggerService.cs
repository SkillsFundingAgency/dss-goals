using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Service
{
    public class GetGoalHttpTriggerService : IGetGoalHttpTriggerService
    {
        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var goals = await documentDbProvider.GetGoalsForCustomerAsync(customerId);

            return goals;
        }
    }
}
