using System;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service
{
    public interface IGetGoalByIdHttpTriggerService
    {
        Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId);
    }
}