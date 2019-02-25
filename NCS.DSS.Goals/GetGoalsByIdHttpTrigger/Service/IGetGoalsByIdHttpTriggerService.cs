using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Service
{
    public interface IGetGoalsByIdHttpTriggerService
    {
        Task<Models.Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalId);
    }
}