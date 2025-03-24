using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Goal.Cosmos.Provider
{
    public interface ICosmosDbProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<string> GetGoalForCustomerToUpdateAsync(Guid customerId, Guid goalId, Guid actionPlanId);
        Task<List<Models.Goal>> GetAllGoalsForCustomerAsync(Guid customerId, Guid actionPlanId);
        Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId);
        Task<ItemResponse<Models.Goal>> CreateGoalAsync(Models.Goal goal);
        Task<ItemResponse<Models.Goal>> UpdateGoalAsync(string goalJson, Guid goalId);
    }
}