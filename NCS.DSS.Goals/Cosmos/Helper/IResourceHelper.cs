using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goals.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> IsCustomerReadOnly(Guid customerId);
        bool DoesInteractionExistAndBelongToCustomer(Guid interactionId, Guid customerGuid);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionplanId, Guid interactionId, Guid customerId);
    }
}