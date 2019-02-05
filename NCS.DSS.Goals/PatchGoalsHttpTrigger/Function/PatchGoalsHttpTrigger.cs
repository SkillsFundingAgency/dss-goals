using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goals.Cosmos.Helper;
using NCS.DSS.Goals.PatchGoalsHttpTrigger.Service;
using NCS.DSS.Goals.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Goals.PatchGoalsHttpTrigger.Function
{
    public static class PatchGoalsHttpTrigger
    {
        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Goal),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goals validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Goals record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Goals/{GoalId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string GoalId, string sessionId,
            [Inject]IResourceHelper resourceHelper, 
            [Inject]IPatchGoalsHttpTriggerService GoalsPatchService,
            [Inject]IValidate validate,
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

            var apimUrl = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'apimurl' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubcontractorId' in request header");

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Post Actions C# HTTP trigger function  processed a request. By Touchpoint: {0}",
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

            if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'actionPlanId' to a Guid: {0}", actionplanId));
                return httpResponseMessageHelper.BadRequest(actionPlanGuid);
            }

            if (!Guid.TryParse(GoalId, out var goalGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'GoalId' to a Guid: {0}", GoalId));
                return httpResponseMessageHelper.BadRequest(goalGuid);
            }

            Models.GoalPatch goalPatchRequest;

            try
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to get resource from body of the request");
                goalPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.GoalPatch>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogError(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (goalPatchRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Goal patch request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to set id's for Goal");
            goalPatchRequest.SetIds(touchpointId);

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(goalPatchRequest, true);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get Session {0} for customer {1}", sessionGuid, customerGuid));
            var doesSessionExist = resourceHelper.DoesSessionResourceExistAndBelongToCustomer(sessionGuid, interactionGuid, customerGuid);

            if (!doesSessionExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Session does not exist {0}", interactionGuid));
                return httpResponseMessageHelper.NoContent(sessionGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get goal {0} for customer {1}", goalGuid, customerGuid));
            var goal = await GoalsPatchService.GetGoalForCustomerAsync(customerGuid, goalGuid);

            if (goal == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Goal does not exist {0}", goalGuid));
                return httpResponseMessageHelper.NoContent(goalGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to update goal {0}", goalGuid));
            var updatedGoal = await GoalsPatchService.UpdateAsync(goal, goalPatchRequest, goalGuid);

            if (updatedGoal != null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("attempting to send to service bus {0}", goalGuid));
                await GoalsPatchService.SendToServiceBusQueueAsync(updatedGoal, customerGuid, apimUrl);
            }

            loggerHelper.LogMethodExit(log);

            return updatedGoal == null ?
                httpResponseMessageHelper.BadRequest(goalGuid) :
                httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(updatedGoal, "id", "GoalId"));

        }
    }
}
