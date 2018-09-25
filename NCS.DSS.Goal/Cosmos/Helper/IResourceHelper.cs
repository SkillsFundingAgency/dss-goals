using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        bool DoesActionPlanExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
    }
}