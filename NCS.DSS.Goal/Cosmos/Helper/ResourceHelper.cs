using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public ResourceHelper(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerHaveATerminationDate(customerId);
        }

        public Task<bool> DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesInteractionExistAndBelongToCustomer(interactionId, customerId);
        }

        public Task<bool> DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            return _cosmosDbProvider.DoesActionPlanExistAndBelongToCustomer(actionPlanId, interactionId, customerId);
        }
    }
}
