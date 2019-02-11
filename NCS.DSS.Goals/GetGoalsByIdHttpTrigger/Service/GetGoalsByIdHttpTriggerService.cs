using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalsByIdHttpTrigger.Service
{
    public class GetGoalsByIdHttpTriggerService : IGetGoalsByIdHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetGoalsByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalGuid)
        {
            var Goals = await _documentDbProvider.GetGoalsForCustomerAsync(customerId, goalGuid);

            return Goals;
        }
    }
}