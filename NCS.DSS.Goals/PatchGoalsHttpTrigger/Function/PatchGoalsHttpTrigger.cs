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

            return httpResponseMessageHelper.Ok("Successfully updated Goals record!");
            
            
            //var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            //if (string.IsNullOrEmpty(touchpointId))
            //{
            //    log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //var ApimURL = httpRequestHelper.GetDssApimUrl(req);
            //if (string.IsNullOrEmpty(ApimURL))
            //{
            //    log.LogInformation("Unable to locate 'apimurl' in request header");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //log.LogInformation("Patch Action Plan C# HTTP trigger function processed a request. " + touchpointId);

            //if (!Guid.TryParse(customerId, out var customerGuid))
            //    return httpResponseMessageHelper.BadRequest(customerGuid);

            //if (!Guid.TryParse(interactionId, out var interactionGuid))
            //    return httpResponseMessageHelper.BadRequest(interactionGuid);

            //if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            //    return httpResponseMessageHelper.BadRequest(actionplanGuid);

            //if (!Guid.TryParse(OutcomeId, out var GoalsGuid))
            //    return httpResponseMessageHelper.BadRequest(GoalsGuid);

            //Models.GoalsPatch  GoalsPatchRequest;

            //try
            //{
            //    GoalsPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.GoalsPatch>(req);
            //}
            //catch (JsonException ex)
            //{
            //    return httpResponseMessageHelper.UnprocessableEntity(ex);
            //}

            //if (GoalsPatchRequest == null)
            //    return httpResponseMessageHelper.UnprocessableEntity(req);

            //GoalsPatchRequest.LastModifiedTouchpointId = touchpointId;

            //var errors = validate.ValidateResource(GoalsPatchRequest);

            //if (errors != null && errors.Any())
            //    return httpResponseMessageHelper.UnprocessableEntity(errors);

            //var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            //if (!doesCustomerExist)
            //    return httpResponseMessageHelper.NoContent(customerGuid);

            //var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            //if (isCustomerReadOnly)
            //    return httpResponseMessageHelper.Forbidden(customerGuid);

            //var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            //if (!doesInteractionExist)
            //    return httpResponseMessageHelper.NoContent(interactionGuid);

            //var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionplanGuid, interactionGuid, customerGuid);

            //if (!doesActionPlanExist)
            //    return httpResponseMessageHelper.NoContent(actionplanGuid);

            //var Goals = await GoalsPatchService.GetGoalsForCustomerAsync(customerGuid, interactionGuid, actionplanGuid, GoalsGuid);

            //if (Goals == null)
            //    return httpResponseMessageHelper.NoContent(GoalsGuid);

            //var updatedGoals = await GoalsPatchService.UpdateAsync(Goals, GoalsPatchRequest);

            //if (updatedGoals != null)
            //    await GoalsPatchService.SendToServiceBusQueueAsync(updatedGoals,customerGuid, ApimURL);

            //return updatedGoals == null ?
            //    httpResponseMessageHelper.BadRequest(GoalsGuid) :
            //    httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(Goals, "id", "OutcomeId"));

        }
    }
}
