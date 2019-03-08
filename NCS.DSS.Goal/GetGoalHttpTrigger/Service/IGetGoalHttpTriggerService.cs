﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Service
{
    public interface IGetGoalHttpTriggerService
    {
        Task<List<Models.Goal>> GetGoalsAsync(Guid customerId, Guid actionPlanId);
    }
}