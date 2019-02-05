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
using NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Service;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Function
{
    public static class GetGoalsByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Goal), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Goals for the given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Goals/{GoalId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string GoalId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetGoalsByIdHttpTriggerService GoalsGetService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

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
                string.Format("Get Action Plan By Id C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'interactionId' to a Guid: {0}", interactionId));
                return httpResponseMessageHelper.BadRequest(interactionGuid);
            }

            if (!Guid.TryParse(sessionId, out var sessionGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'sessionId' to a Guid: {0}", sessionGuid));
                return httpResponseMessageHelper.BadRequest(sessionGuid);
            }

            if (!Guid.TryParse(GoalId, out var goalGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionPlanId' to a Guid: {0}", GoalId));
                return httpResponseMessageHelper.BadRequest(goalGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Session {0} for customer {1}", sessionGuid, customerGuid));
            var doesSessionExist = resourceHelper.DoesSessionResourceExistAndBelongToCustomer(sessionGuid, interactionGuid, customerGuid);

            if (!doesSessionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Session does not exist {0}", sessionGuid));
                return httpResponseMessageHelper.NoContent(sessionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Goal {0} for customer {1}", goalGuid, customerGuid));
            var goal = await GoalsGetService.GetGoalsForCustomerAsync(customerGuid, goalGuid);

            loggerHelper.LogMethodExit(log);

            return goal == null ?
                httpResponseMessageHelper.NoContent(customerGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(goal, "id", "GoalId"));

        }
    }
}