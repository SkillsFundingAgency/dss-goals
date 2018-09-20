using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.Cosmos.Helper
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

        public async Task<bool> DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesIntergoalExist = await documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesIntergoalExist;
        }

        public async Task<bool> DoesActionPlanExist(Guid actionPlanId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExist = await documentDbProvider.DoesActionPlanResourceExist(actionPlanId);

            return doesActionPlanExist;
        }
    }
}
