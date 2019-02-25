using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Goals.Models;

namespace NCS.DSS.Goals.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Goal>> GetAllGoalsForCustomerAsync(Guid customerId);
        Task<Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalId);

        Task<string> GetGoalForCustomerToUpdateAsync(Guid customerId, Guid goalId);
        Task<ResourceResponse<Document>> CreateGoalsAsync(Goal Goals);
        Task<ResourceResponse<Document>> UpdateGoalAsync(string goalJson, Guid goalId);

        Task<bool> DeleteAsync(Guid goalId);
        bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId);

    }
}