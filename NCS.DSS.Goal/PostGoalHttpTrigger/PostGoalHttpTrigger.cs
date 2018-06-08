using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Goal.PostGoalHttpTrigger
{
    public static class PostGoalHttpTrigger
    {
        [FunctionName("Post")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Interactions/{interactionId:guid}/ActionPlans/{actionplanId:guid}/Goals/")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("Post Goal C# HTTP trigger function processed a request.");

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}