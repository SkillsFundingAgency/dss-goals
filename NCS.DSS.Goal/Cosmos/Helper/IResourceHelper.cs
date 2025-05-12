namespace NCS.DSS.Goal.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
    }
}