using System;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;

namespace NCS.DSS.Goals.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = await documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var isCustomerReadOnly = await documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }

       public bool DoesSessionResourceExistAndBelongToCustomer(Guid sessionId, Guid interactionId, Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesInteractionExist = documentDbProvider.DoesSessionResourceExistAndBelongToCustomer(sessionId, interactionId, customerId);

            return doesInteractionExist;
        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExist = documentDbProvider.DoesActionPlanResourceExistAndBelongToCustomer(actionplanId, interactionId, customerId);

            return doesActionPlanExist;
        }
    }
}
