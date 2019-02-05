using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goals.Cosmos.Helper;
using NCS.DSS.Goals.GetGoalsHttpTrigger.Service;
using Microsoft.AspNetCore.Mvc;
using DFC.Swagger.Standard.Annotations;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.Goals.GetGoalsHttpTrigger.Function
{
    public static class GetGoalsHttpTrigger
    {
        [FunctionName("Get")]
        [ProducesResponseType(typeof(Models.Goal),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all Goals for the given Interactions.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Goals")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetGoalsHttpTriggerService GoalsGetService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Get Action Plan C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return httpResponseMessageHelper.BadRequest(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get goals for customer {0}", customerGuid));
            var goals = await GoalsGetService.GetGoalsAsync(customerGuid);

            loggerHelper.LogMethodExit(log);

            return goals == null ?
                httpResponseMessageHelper.NoContent(customerGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectsAndRenameIdProperty(goals, "id", "GoalId"));

        }
    }
}