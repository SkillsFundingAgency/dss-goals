using System.Threading.Tasks;

namespace NCS.DSS.Goals.PostGoalsHttpTrigger.Service
{
    public interface IPostGoalsHttpTriggerService
    {
        Task<Models.Goal> CreateAsync(Models.Goal Goals);
        Task SendToServiceBusQueueAsync(Models.Goal Goals, string reqUrl);
    }
}