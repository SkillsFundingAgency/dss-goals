using System;
using NCS.DSS.Goal.Cosmos.Provider;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public bool DoesInteractionExist(Guid interactionId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesIntergoalExist = documentDbProvider.DoesInteractionResourceExist(interactionId);

            return doesIntergoalExist;
        }

        public bool DoesActionPlanExist(Guid actionPlanId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesActionPlanExist = documentDbProvider.DoesActionPlanResourceExist(actionPlanId);

            return doesActionPlanExist;
        }
    }
}
