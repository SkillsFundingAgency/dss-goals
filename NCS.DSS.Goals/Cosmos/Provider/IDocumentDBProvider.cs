using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Goal.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Models.Goal>> GetAllGoalsForCustomerAsync(Guid customerId);
        Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalId);

        Task<string> GetGoalForCustomerToUpdateAsync(Guid customerId, Guid goalId);
        Task<ResourceResponse<Document>> CreateGoalsAsync(Models.Goal Goals);
        Task<ResourceResponse<Document>> UpdateGoalsAsync(Models.Goal goal);

        Task<bool> DeleteAsync(Guid OutcomeId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);

    }
}