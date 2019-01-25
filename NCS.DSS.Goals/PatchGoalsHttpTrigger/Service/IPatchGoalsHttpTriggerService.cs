using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public interface IPatchGoalsHttpTriggerService
    {
        Task<Models.Goal> UpdateAsync(Models.Goal Goals, Models.GoalPatch goalsPatch);
        Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid interactionsId, Guid actionplanId, Guid OutcomeId);
        Task SendToServiceBusQueueAsync(Models.Goal Goals, Guid customerId, string reqUrl);
    }
}