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

        public bool DoesActionPlanExist(Guid goalPlanId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesGoalPlanExist = documentDbProvider.DoesActionPlanResourceExist(goalPlanId);

            return doesGoalPlanExist;
        }
    }
}
