using NCS.DSS.Goals.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public interface IGoalsPatchService
    {
        string Patch(string goalsJson, GoalPatch goalPatch);
    }
}
