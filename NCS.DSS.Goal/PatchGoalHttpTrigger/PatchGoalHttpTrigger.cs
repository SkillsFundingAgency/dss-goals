using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger
{
    public static class PatchGoalHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Goal))]
        [Display(Name = "Patch", Description = "Ability to update an existing goal record")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Goals/{goalId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string goalId)
        {
            log.Info("Patch Goal C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Updated Goal record with Id of : " + goalGuid)
            };
        }
    }
}