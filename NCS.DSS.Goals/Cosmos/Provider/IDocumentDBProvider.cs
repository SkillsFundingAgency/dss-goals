using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Goals.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Models.Goal>> GetGoalsForCustomerAsync(Guid customerId);
        Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid outcomeId);
        Task<ResourceResponse<Document>> CreateGoalsAsync(Models.Goal Goals);
        Task<ResourceResponse<Document>> UpdateGoalsAsync(Models.Goal Goals);
        Task<bool> DeleteAsync(Guid OutcomeId);
    }
}