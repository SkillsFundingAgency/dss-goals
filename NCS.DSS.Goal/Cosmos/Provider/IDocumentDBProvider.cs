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
        Task<bool> DoesInteractionResourceExist(Guid interactionId);
        Task<bool> DoesActionPlanResourceExist(Guid actionPlanId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<List<Models.Goal>> GetGoalsForCustomerAsync(Guid customerId);
        Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId);
        Task<ResourceResponse<Document>> CreateGoalAsync(Models.Goal goal);
        Task<ResourceResponse<Document>> UpdateGoalAsync(Models.Goal goal);
    }
}