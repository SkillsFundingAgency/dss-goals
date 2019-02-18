using NCS.DSS.Goal.Models;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.PatchGoalsHttpTrigger.Service
{
    public interface IPatchGoalsHttpTriggerService
    {

        Task<Models.Goal> UpdateCosmosAsync(Models.Goal goal);

        Task<string> GetGoalForCustomerAsync(Guid customerId, Guid goalId);
        
        Task SendToServiceBusQueueAsync(Models.Goal Goals, Guid customerId, string reqUrl);

        Models.Goal PatchResource(string goalJson, GoalPatch goalPatch);
    }
}