using NCS.DSS.Goals.Models;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Service
{
    public interface IGoalsPatchService
    {
        string Patch(string goalsJson, GoalPatch goalPatch);
    }
}
