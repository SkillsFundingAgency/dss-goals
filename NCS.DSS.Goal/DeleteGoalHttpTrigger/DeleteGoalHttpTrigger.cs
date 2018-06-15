using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Goal.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.DeleteGoalHttpTrigger
{
    public static class DeleteGoalHttpTrigger
    {
        [FunctionName("Delete")]
        [GoalResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goal Deleted", ShowSchema = true)]
        [GoalResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Goal Id does not exist", ShowSchema = false)]
        [Display(Name = "Delete", Description = "Ability to delete a goal record.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Goals/{goalId}")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId, string goalId)
        {
            log.Info("Delete Goal C# HTTP trigger function processed a request.");

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
                Content = new StringContent("Deleted Goal record with Id of : " + goalGuid)
            };
        }
    }
}