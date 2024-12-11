using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Service
{
    public class GetGoalHttpTriggerService : IGetGoalHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetGoalHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId, Guid actionPlanId)
        {
            return await _cosmosDbProvider.GetAllGoalsForCustomerAsync(customerId, actionPlanId);
        }
    }
}
