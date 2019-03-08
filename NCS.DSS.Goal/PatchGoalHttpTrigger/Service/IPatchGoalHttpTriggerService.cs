using System;
using System.Threading.Tasks;
using NCS.DSS.Goal.Models;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public interface IPatchGoalHttpTriggerService
    {
        string PatchResource(string goalJson, GoalPatch goalPatch);
        Task<Models.Goal> UpdateCosmosAsync(string goalJson, Guid goalId);
        Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId, Guid actionPlanId);
        Task SendToServiceBusQueueAsync(Models.Goal goal, Guid customerId, string reqUrl);
    }
}