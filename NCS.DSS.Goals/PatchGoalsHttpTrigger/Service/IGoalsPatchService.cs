using DFC.JSON.Standard;
using NCS.DSS.Goal.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NCS.DSS.Goal.PatchGoalsHttpTrigger.Service
{
    public interface IGoalsPatchService
    {
        Models.Goal Patch(string goalsJson, GoalPatch goalPatch);
    }
}
