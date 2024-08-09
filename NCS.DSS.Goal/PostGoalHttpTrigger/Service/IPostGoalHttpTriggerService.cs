namespace NCS.DSS.Goal.PostGoalHttpTrigger.Service
{
    public interface IPostGoalHttpTriggerService
    {
        Task<Models.Goal> CreateAsync(Models.Goal goal);
        Task SendToServiceBusQueueAsync(Models.Goal goal, string reqUrl);
    }
}