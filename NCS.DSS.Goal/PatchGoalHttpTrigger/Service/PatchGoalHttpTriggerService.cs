using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.ServiceBus;
using System.Net;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public class PatchGoalHttpTriggerService : IPatchGoalHttpTriggerService
    {

        private readonly IGoalPatchService _goalPatchService;
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IGoalServiceBusClient _goalServiceBusClient;
        private readonly ILogger<PatchGoalHttpTriggerService> _logger;

        public PatchGoalHttpTriggerService(IGoalPatchService goalPatchService, ICosmosDbProvider cosmosDbProvider, IGoalServiceBusClient goalServiceBusClient, ILogger<PatchGoalHttpTriggerService> logger)
        {
            _goalPatchService = goalPatchService;
            _cosmosDbProvider = cosmosDbProvider;
            _goalServiceBusClient = goalServiceBusClient;
            _logger = logger;
        }

        public string PatchResource(string goalJson, GoalPatch goalPatch)
        {
            if (string.IsNullOrEmpty(goalJson))
            {
                _logger.LogInformation("Invalid input: goalJson is null or empty.");
                return null;
            }

            if (goalPatch == null)
            {
                _logger.LogInformation("Invalid input: goalPatch object is null.");
                return null;
            }

            _logger.LogInformation("Setting default values for GoalPatch object.");
            goalPatch.SetDefaultValues();

            _logger.LogInformation("Attempting to patch the Goal resource.");
            var updatedGoal = _goalPatchService.Patch(goalJson, goalPatch);

            if (updatedGoal != null)
            {
                _logger.LogInformation("Successfully patched the Goal resource.");
            }

            return updatedGoal;
        }

        public async Task<Models.Goal> UpdateCosmosAsync(string goalJson, Guid goalId)
        {
            if (string.IsNullOrEmpty(goalJson))
            {
                _logger.LogInformation("The goal object provided is either null or empty.");
                return null;
            }

            _logger.LogInformation("Patching goal with ID: {GoalId}.", goalId);
            var response = await _cosmosDbProvider.UpdateGoalAsync(goalJson, goalId);

            if (response?.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Successfully updated goal with ID: {GoalId}.", goalId);
                return response.Resource;
            }

            _logger.LogError("Failed to update goal with ID: {GoalId}.", goalId);
            return null;
        }

        public async Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving goal with ID: {GoalId} for customer ID: {CustomerId} and actionPlan ID: {ActionPlanId}.", goalId, customerId, actionPlanId);

            var goal = await _cosmosDbProvider.GetGoalForCustomerToUpdateAsync(customerId, goalId, actionPlanId);

            if (goal == null)
            {
                _logger.LogWarning("No goal found with ID: {GoalId} for customer ID: {CustomerId}.", goalId, customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved goal with ID: {GoalId} for customer ID: {CustomerId} and actionPlan ID: {ActionPlanId}.", goalId, customerId, actionPlanId);
            }

            return goal;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal goal, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, customerId);

                await _goalServiceBusClient.SendPatchMessageAsync(goal, customerId, reqUrl);

                _logger.LogInformation("Successfully sent goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, customerId);
                throw;
            }
        }
    }
}