using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Goal.PostGoalHttpTrigger
{
    public static class PostGoalHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Goal))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Goals/")]HttpRequestMessage req, TraceWriter log)
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