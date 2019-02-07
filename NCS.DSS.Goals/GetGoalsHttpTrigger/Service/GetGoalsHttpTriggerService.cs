using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalsHttpTrigger.Service
{
    public class GetGoalsHttpTriggerService : IGetGoalsHttpTriggerService
    {
        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var Goals = await documentDbProvider.GetGoalsForCustomerAsync(customerId);

            return Goals;
        }
    }
}