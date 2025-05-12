namespace NCS.DSS.Goal.ServiceBus
{
    public interface IGoalServiceBusClient
    {
        Task SendPatchMessageAsync(Models.Goal goals, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.Goal goals, string reqUrl);
    }
}