using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service
{
    public class GetGoalByIdHttpTriggerService : IGetGoalByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetGoalByIdHttpTriggerService> _logger;

        public GetGoalByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetGoalByIdHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving goal with ID: {GoalId} for customer ID: {CustomerId} and action plan ID: {ActionPlanId}.", goalId, customerId, actionPlanId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            if (goalId == Guid.Empty)
            {
                _logger.LogWarning("Invalid goal ID provided: {GoalId}.", goalId);
                return null;
            }

            if (actionPlanId == Guid.Empty)
            {
                _logger.LogWarning("Invalid action plan ID provided: {ActionPlanId}.", actionPlanId);
                return null;
            }

            var goal = await _cosmosDbProvider.GetGoalForCustomerAsync(customerId, goalId, actionPlanId);

            if (goal == null)
            {
                _logger.LogInformation("No goal found with ID: {GoalId} for customer ID: {CustomerId} and action plan ID: {ActionPlanId}", goalId, customerId, actionPlanId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved goal with ID: {GoalId} for customer ID: {CustomerId} and action plan ID: {ActionPlanId}", goalId, customerId, actionPlanId);
            }

            return goal;
        }
    }
}