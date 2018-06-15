using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Goal.Annotations;

namespace NCS.DSS.Goal.PostGoalHttpTrigger
{
    public static class PostGoalHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Goal))]
        [GoalResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Goal Created", ShowSchema = true)]
        [GoalResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to create Goal", ShowSchema = false)]
        [GoalResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a goal for a given action plan.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionplanId}/Goals/")]HttpRequestMessage req, TraceWriter log, string customerId, string interactionId, string actionplanId)
        {
            log.Info("Post Goal C# HTTP trigger function processed a request.");

            // Get request body
            var goal = await req.Content.ReadAsAsync<Models.Goal>();

            var goalService = new PostGoalHttpTriggerService();
            var goalId = goalService.Create(goal);

            return goalId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Goal record with Id of : " + goalId)
                };
        }
    }
}