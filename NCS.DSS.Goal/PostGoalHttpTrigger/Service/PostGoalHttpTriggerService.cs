using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.ServiceBus;
using System.Net;

namespace NCS.DSS.Goal.PostGoalHttpTrigger.Service
{
    public class PostGoalHttpTriggerService : IPostGoalHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IGoalServiceBusClient _goalServiceBusClient;
        private readonly ILogger<PostGoalHttpTriggerService> _logger;

        public PostGoalHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IGoalServiceBusClient goalServiceBusClient, ILogger<PostGoalHttpTriggerService> logger)
        {

            _cosmosDbProvider = cosmosDbProvider;
            _goalServiceBusClient = goalServiceBusClient;
            _logger = logger;
        }

        public async Task<Models.Goal> CreateAsync(Models.Goal goal)
        {
            if (goal == null)
            {
                _logger.LogInformation("The goal object provided is null.");
                return null;
            }

            goal.SetDefaultValues();

            _logger.LogInformation("Attempting to create goal with ID: {GoalId}.", goal.GoalId);
            var response = await _cosmosDbProvider.CreateGoalAsync(goal);

            if (response?.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Successfully created goal with ID: {GoalId}.", goal.GoalId);
                return response.Resource;
            }

            _logger.LogError("Failed to create goal with ID: {GoalId}.", goal.GoalId);
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal goal, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending newly created goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, goal.CustomerId);

                await _goalServiceBusClient.SendPostMessageAsync(goal, reqUrl);

                _logger.LogInformation("Successfully sent goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, goal.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending goal with ID: {GoalId} to Service Bus for customer ID: {CustomerId}.", goal.GoalId, goal.CustomerId);
                throw;
            }
        }
    }
}