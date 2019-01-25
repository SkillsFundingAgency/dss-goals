using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;

namespace NCS.DSS.Goals.GetGoalsHttpTrigger.Service
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