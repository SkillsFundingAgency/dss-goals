using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.ServiceBus;

namespace NCS.DSS.Goal.PostGoalsHttpTrigger.Service
{
    public class PostGoalsHttpTriggerService : IPostGoalsHttpTriggerService
    {
        public async Task<Models.Goal> CreateAsync(Models.Goal Goals)
        {
            if (Goals == null)
                return null;

            Goals.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateGoalsAsync(Goals);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal Goals, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(Goals, reqUrl);
        }
    }
}