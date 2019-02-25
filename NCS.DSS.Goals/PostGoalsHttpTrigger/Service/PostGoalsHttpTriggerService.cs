using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goals.ServiceBus;
using NCS.DSS.Goals.Cosmos.Provider;

namespace NCS.DSS.Goals.PostGoalsHttpTrigger.Service
{
    public class PostGoalsHttpTriggerService : IPostGoalsHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public PostGoalsHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Goal> CreateAsync(Models.Goal Goals)
        {
            if (Goals == null)
                return null;

            Goals.SetDefaultValues();

            var response = await _documentDbProvider.CreateGoalsAsync(Goals);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal Goals, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(Goals, reqUrl);
        }
    }
}