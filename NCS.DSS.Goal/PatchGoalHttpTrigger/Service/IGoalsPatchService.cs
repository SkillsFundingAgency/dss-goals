using NCS.DSS.Goal.Models;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Service
{
    public interface IGoalPatchService
    {
        string Patch(string goalsJson, GoalPatch goalPatch);
    }
}
