using NCS.DSS.Goal.Cosmos.Provider;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service
{
    public class GetGoalByIdHttpTriggerService : IGetGoalByIdHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetGoalByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            return await _documentDbProvider.GetGoalForCustomerAsync(customerId, goalId, actionPlanId);
        }
    }
}