using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.PutGoalHttpTrigger
{
    public static class PutGoalHttpTrigger
    {
        [FunctionName("Put")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Goals/{goalId:guid}")]HttpRequestMessage req, TraceWriter log, string goalId)
        {
            log.Info("Put Goal C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(goalId, out var goalGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(goalId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Replaced Goal record with Id of : " + goalGuid)
            };
        }
    }
}