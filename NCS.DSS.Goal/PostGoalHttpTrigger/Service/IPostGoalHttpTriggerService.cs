using System.Threading.Tasks;

namespace NCS.DSS.Goal.PostGoalHttpTrigger.Service
{
    public interface IPostGoalHttpTriggerService
    {
        Task<Models.Goal> CreateAsync(Models.Goal goal);
    }
}