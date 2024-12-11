using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Service
{
    public class GetGoalHttpTriggerService : IGetGoalHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetGoalHttpTriggerService> _logger;

        public GetGoalHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetGoalHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving goals for customer ID: {CustomerId} and action plan ID: {ActionPlanId}.", customerId, actionPlanId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            if (actionPlanId == Guid.Empty)
            {
                _logger.LogWarning("Invalid action plan ID provided: {ActionPlanId}.", actionPlanId);
                return null;
            }

            var goals = await _cosmosDbProvider.GetAllGoalsForCustomerAsync(customerId, actionPlanId);

            if (goals == null)
            {
                _logger.LogInformation("No goals found for customer ID: {CustomerId} and action plan ID: {ActionPlanId}.", customerId, actionPlanId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved goal(s) for customer ID: {CustomerId} and action plan ID: {ActionPlanId}.", customerId, actionPlanId);
            }

            return goals;
        }
    }
}
