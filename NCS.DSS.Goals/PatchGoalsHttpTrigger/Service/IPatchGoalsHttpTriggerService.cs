using NCS.DSS.Goals.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public interface IPatchGoalsHttpTriggerService
    {
        string PatchResource(string goalJson, GoalPatch goalPatch);
        Task<Goal> UpdateCosmosAsync(string goalJson, Guid goalId);
        Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId);
        Task SendToServiceBusQueueAsync(Models.Goal goal, Guid customerId, string reqUrl);
    }
}