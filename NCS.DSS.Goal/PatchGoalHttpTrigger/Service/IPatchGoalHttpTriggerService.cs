using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public interface IPatchGoalHttpTriggerService
    {
        Task<Models.Goal> UpdateAsync(Models.Goal goal, Models.GoalPatch goalPatch);
        Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId);
        Task SendToServiceBusQueueAsync(Models.Goal goal, Guid customerId, string reqUrl);
    }
}