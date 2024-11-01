﻿using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.ServiceBus;
using System.Net;

namespace NCS.DSS.Goal.PostGoalHttpTrigger.Service
{
    public class PostGoalHttpTriggerService : IPostGoalHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public PostGoalHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Goal> CreateAsync(Models.Goal goal)
        {
            if (goal == null)
                return null;

            goal.SetDefaultValues();

            var response = await _documentDbProvider.CreateGoalAsync(goal);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Goal goal, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(goal, reqUrl);
        }
    }
}