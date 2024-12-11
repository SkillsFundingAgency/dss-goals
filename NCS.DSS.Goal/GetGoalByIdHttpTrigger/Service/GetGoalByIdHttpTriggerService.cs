using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service
{
    public class GetGoalByIdHttpTriggerService : IGetGoalByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetGoalByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            return await _cosmosDbProvider.GetGoalForCustomerAsync(customerId, goalId, actionPlanId);
        }
    }
}