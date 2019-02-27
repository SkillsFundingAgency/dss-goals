using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goals.GoalChangeFeedTrigger.Service;
using Newtonsoft.Json;

namespace NCS.DSS.Goals.GoalChangeFeedTrigger.Function
{
    public static class GoalChangeFeedTrigger
    {
        private const string DatabaseName = "%DatabaseId%";
        private const string CollectionName = "%CollectionId%";
        private const string ConnectionString = "GoalConnectionString";
        private const string LeaseCollectionName = "%LeaseCollectionName%";
        private const string LeaseCollectionPrefix = "%LeaseCollectionPrefix%";

        [FunctionName("GoalChangeFeedTrigger")]
        public static async Task Run([CosmosDBTrigger(
                DatabaseName,
                CollectionName,
                ConnectionStringSetting = ConnectionString,
                LeaseCollectionName = LeaseCollectionName,
                LeaseCollectionPrefix = LeaseCollectionPrefix,
                CreateLeaseCollectionIfNotExists = true
            )]IReadOnlyList<Document> documents, ILogger log,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IGoalChangeFeedTriggerService changeFeedTriggerService)
        {
            loggerHelper.LogMethodEnter(log);

            try
            {
                foreach (var document in documents)
                {
                    loggerHelper.LogInformationMessage(log, Guid.NewGuid(), string.Format("Attempting to send document id: {0} to service bus queue", document.Id));
                    await changeFeedTriggerService.SendMessageToChangeFeedQueueAsync(document);
                }
            }
            catch (Exception ex)
            {
                loggerHelper.LogException(log, Guid.NewGuid(), "Error when trying to add document to service bus queue", ex);
            }

            loggerHelper.LogMethodExit(log);
        }
    }
}
