using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public ResourceHelper(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerHaveATerminationDate(customerId);
        }

        public bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesInteractionExistAndBelongToCustomer(interactionId, customerId);
        }

        public bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            return _documentDbProvider.DoesActionPlanExistAndBelongToCustomer(actionPlanId, interactionId, customerId); 
        }
    }
}
