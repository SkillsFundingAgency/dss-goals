using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalsHttpTrigger.Service
{
    public interface IGetGoalsHttpTriggerService
    {
        Task<List<Models.Goal>> GetGoalsAsync(Guid customerId);
    }
}