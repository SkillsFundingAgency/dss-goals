using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Goal.Annotations;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger
{
    public static class GetGoalByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goal found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goal does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Goals/{goalId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string goalId)
        {
            log.Info("Get Goal By Id C# HTTP trigger function  processed a request.");

            if (!Guid.TryParse(goalId, out var goalGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(goalId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var service = new GetGoalByIdHttpTriggerService();
            var values = await service.GetGoal(goalGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}