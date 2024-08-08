using NCS.DSS.Goal.Cosmos.Provider;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Service
{
    public class GetGoalHttpTriggerService : IGetGoalHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetGoalHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId, Guid actionPlanId)
        {
            return await _documentDbProvider.GetAllGoalsForCustomerAsync(customerId, actionPlanId);
        }
    }
}
