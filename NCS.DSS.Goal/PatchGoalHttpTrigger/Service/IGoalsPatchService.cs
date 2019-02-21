using NCS.DSS.Goal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public interface IGoalsPatchService
    {
        string Patch(string goalsJson, GoalPatch goalPatch);
    }
}
