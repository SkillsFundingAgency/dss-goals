using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Goal.Models;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.Cosmos.Provider
{
    public class CosmosDbProvider : ICosmosDbProvider
    {
        private readonly Container _goalContainer;
        private readonly Container _customerContainer;
        private readonly Container _interactionContainer;
        private readonly Container _actionPlanContainer;
        private readonly ILogger<CosmosDbProvider> _logger;

        public CosmosDbProvider(CosmosClient cosmosClient,
            IOptions<GoalConfigurationSettings> configOptions,
            ILogger<CosmosDbProvider> logger)
        {
            var config = configOptions.Value;

            _goalContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _interactionContainer = GetContainer(cosmosClient, config.InteractionDatabaseId, config.InteractionCollectionId);
            _actionPlanContainer = GetContainer(cosmosClient, config.ActionPlanDatabaseId, config.ActionPlanCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for customer resource. Customer ID: {CustomerId}", customerId);

                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                if (response.Resource != null)
                {
                    _logger.LogInformation("Customer exists. Customer ID: {CustomerId}", customerId);
                    return true;
                }

                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for interaction resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);

                string queryText = "SELECT VALUE COUNT(1) FROM interactions i WHERE i.id = @interactionId AND i.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _interactionContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var interactionFound = response.FirstOrDefault() > 0;

                    if (interactionFound)
                    {
                        _logger.LogInformation("Interaction for customer exists. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                    }
                    return interactionFound;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Interaction for customer is not found. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking interaction resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId}", customerId, interactionId);
                throw;
            }
        }

        public async Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            _logger.LogInformation("Checking for action plan resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
            try
            {
                string queryText = "SELECT VALUE COUNT(1) FROM actionplans a WHERE a.id = @actionPlanId AND a.InteractionId = @interactionId AND a.CustomerId = @customerId";
                var queryDefinition = new QueryDefinition(queryText)
                    .WithParameter("@actionPlanId", actionPlanId.ToString())
                    .WithParameter("@interactionId", interactionId.ToString())
                    .WithParameter("@customerId", customerId.ToString());

                using var iterator = _actionPlanContainer.GetItemQueryIterator<dynamic>(queryDefinition);

                if (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    var actionPlanExists = response.FirstOrDefault() > 0;
                    if (actionPlanExists)
                    {
                        _logger.LogInformation("Action plan for customer exists. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                    }
                    return actionPlanExists;
                }

                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Action plan for customer is not found. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking action plan resource for a customer. Customer ID: {CustomerId} Interaction ID: {InteractionId} ActionPlan ID: {ActionPlanId}", customerId, interactionId, actionPlanId);
                throw;
            }
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            _logger.LogInformation("Checking for termination date. Customer ID: {CustomerId}", customerId);

            try
            {
                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                var dateOfTermination = response.Resource?.DateOfTermination;
                var hasTerminationDate = dateOfTermination != null;

                _logger.LogInformation("Termination date check completed. CustomerId: {CustomerId}. HasTerminationDate: {HasTerminationDate}", customerId, hasTerminationDate);
                return hasTerminationDate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking termination date. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<string> GetGoalForCustomerToUpdateAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            var goal = await GetGoalForCustomerAsync(customerId, goalId, actionPlanId);

            return JsonConvert.SerializeObject(goal);
        }

        public async Task<List<Models.Goal>> GetAllGoalsForCustomerAsync(Guid customerId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving Goals for Customer. Customer ID: {CustomerId}. ActionPlan ID: {ActionPlanId}.", customerId, actionPlanId);

            try
            {
                var goals = new List<Models.Goal>();
                var query = _goalContainer.GetItemLinqQueryable<Models.Goal>()
                    .Where(x => x.CustomerId == customerId && x.ActionPlanId == actionPlanId)
                    .ToFeedIterator();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    goals.AddRange(response);
                }

                _logger.LogInformation("Retrieved {Count} Goal(s) for Customer with ID: {CustomerId}. ActionPlan ID: {ActionPlanId}", goals.Count, customerId, actionPlanId);
                return goals;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Goal. Customer ID: {CustomerId}. ActionPlan ID: {ActionPlanId}", customerId, actionPlanId);
                throw;
            }
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId)
        {
            _logger.LogInformation("Retrieving Goal for Customer. Customer ID: {CustomerId}. Goal ID: {GoalId}. ActionPlan ID: {ActionPlanId}", customerId, goalId, actionPlanId);

            try
            {
                var query = _goalContainer.GetItemLinqQueryable<Models.Goal>()
                    .Where(x => x.CustomerId == customerId && x.GoalId == goalId && x.ActionPlanId == actionPlanId)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                if (response.Any())
                {
                    _logger.LogInformation("Goal retrieved successfully. Customer ID: {CustomerId}. Goal ID: {GoalId}. ActionPlan ID: {ActionPlanId}", customerId, goalId, actionPlanId);
                    return response?.FirstOrDefault();
                }

                _logger.LogWarning("Goal not found. Customer ID: {CustomerId}. Goal ID: {GoalId}. ActionPlan ID: {ActionPlanId}", customerId, goalId, actionPlanId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Goal. Customer ID: {CustomerId}. Goal ID: {GoalId}. ActionPlan ID: {ActionPlanId}", customerId, goalId, actionPlanId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Goal>> CreateGoalAsync(Models.Goal goal)
        {
            if (goal == null)
            {
                _logger.LogError("Goal object is null. Creation aborted.");
                throw new ArgumentNullException(nameof(goal), "Goal cannot be null.");
            }

            _logger.LogInformation("Creating Goal with ID: {GoalId}", goal.GoalId);

            try
            {
                var response = await _goalContainer.CreateItemAsync(goal, PartitionKey.None);
                _logger.LogInformation("Successfully created Goal with ID: {GoalID}", goal.GoalId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Goal with ID: {GoalId}", goal.GoalId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Goal>> UpdateGoalAsync(string goalJson, Guid goalId)
        {
            if (string.IsNullOrEmpty(goalJson))
            {
                _logger.LogError("goalJson object is null. Update aborted.");
                throw new ArgumentNullException(nameof(goalJson), "Interaction cannot be null.");
            }

            var goal = JsonConvert.DeserializeObject<Models.Goal>(goalJson);

            _logger.LogInformation("Updating Goal with ID: {GoalId}", goalId);

            try
            {
                var response = await _goalContainer.ReplaceItemAsync(goal, goalId.ToString());
                _logger.LogInformation("Successfully updated Goal with ID: {GoalId}", goalId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Goal with ID: {GoalId}", goalId);
                throw;
            }
        }

    }
}