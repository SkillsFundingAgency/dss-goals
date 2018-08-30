using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionExist(Guid interactionId);
        bool DoesActionPlanExist(Guid actionPlanId);
    }
}