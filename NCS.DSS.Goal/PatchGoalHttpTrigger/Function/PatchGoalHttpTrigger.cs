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
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Function
{
    public class PatchGoalHttpTrigger
    {
        private IResourceHelper resourceHelper;
        private readonly IPatchGoalHttpTriggerService goalsPatchService;
        private ILoggerHelper loggerHelper;
        private IHttpRequestHelper httpRequestHelper;
        private IHttpResponseMessageHelper httpResponseMessageHelper;
        private IJsonHelper jsonHelper;
        private IValidate validate;
        public PatchGoalHttpTrigger(IResourceHelper _resourceHelper, IHttpRequestHelper _httpRequestHelper, IPatchGoalHttpTriggerService _goalsPatchService, IHttpResponseMessageHelper _httpResponseMessageHelper, IJsonHelper _jsonHelper, ILoggerHelper _loggerHelper, IValidate _validate)
        {
            resourceHelper = _resourceHelper;
            httpRequestHelper = _httpRequestHelper;
            goalsPatchService = _goalsPatchService;
            httpResponseMessageHelper = _httpResponseMessageHelper;
            jsonHelper = _jsonHelper;
            loggerHelper = _loggerHelper;
            validate = _validate;
        }

        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Goal), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goals validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Goals record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals/{goalId}")] HttpRequest req, ILogger log, string customerId, string interactionId, string actionPlanId, string goalId)
        {

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            log.LogInformation($"DssCorrelationId: [{correlationGuid}]");

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'TouchpointId' in request header");
                return response;
            }

            var apimUrl = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'apimurl' in request header");
                return response;
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                log.LogInformation($"Unable to locate 'SubcontractorId' in request header");

            log.LogInformation($"Post Actions C# HTTP trigger function  processed a request. By Touchpoint: [{touchpointId}]");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'customerId' to a Guid: [{customerId}]");
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = httpResponseMessageHelper.BadRequest(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'interactionId' to a Guid: [{interactionId}]");
                return response;
            }

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
            {
                var response = httpResponseMessageHelper.BadRequest(actionPlanGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'actionPlanId' to a Guid: [{actionPlanId}]");
                return response;
            }

            if (!Guid.TryParse(goalId, out var goalGuid))
            {
                var response = httpResponseMessageHelper.BadRequest(goalGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'GoalId' to a Guid: [{goalId}]");
                return response;
            }

            GoalPatch goalPatchRequest;

            try
            {
                log.LogInformation($"Attempt to get resource from body of the request");
                goalPatchRequest = await httpRequestHelper.GetResourceFromRequest<GoalPatch>(req);
            }
            catch (JsonException ex)
            {
                var response = httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogError($"Response Status Code: [{response.StatusCode}]. Unable to retrieve body from req", ex);
                return response;
            }

            if (goalPatchRequest == null)
            {
                var response = httpResponseMessageHelper.UnprocessableEntity(req);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Goal patch request is null");
                return response;
            }

            log.LogInformation($"Attempt to set id's for Goal");
            goalPatchRequest.SetIds(touchpointId, subcontractorId);

            log.LogInformation($"Attempt to validate resource");
            var errors = validate.ValidateResource(goalPatchRequest, false);

            if (errors != null && errors.Any())
            {
                var response = httpResponseMessageHelper.UnprocessableEntity(errors);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. validation errors with resource", errors);
                return response;
            }

            log.LogInformation($"Attempting to see if customer exists [{customerGuid}]");
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer does not exist [{customerGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to see if this is a read only customer [{customerGuid}]");
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = httpResponseMessageHelper.Forbidden(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer is read only [{customerGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to get Interaction [{interactionGuid}] for customer [{customerGuid}]");
            var doesInteractionExist = resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = httpResponseMessageHelper.NoContent(interactionGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Interaction does not exist [{interactionGuid}]");
                return response;
            }

            var doesActionPlanExistAndBelongToCustomer = resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
            {
                var response = httpResponseMessageHelper.NoContent(actionPlanGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Action Plan does not exist [{actionPlanGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to get goal [{goalGuid}] for customer [{customerGuid}]");
            var goalForCustomer = await goalsPatchService.GetGoalForCustomerAsync(customerGuid, goalGuid, actionPlanGuid);

            if (goalForCustomer == null)
            {
                var response = httpResponseMessageHelper.NoContent(goalGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Goal does not exist [{goalGuid}]");
                return response;
            }

            var patchedGoal = goalsPatchService.PatchResource(goalForCustomer, goalPatchRequest);

            if (patchedGoal == null)
            {
                var response = httpResponseMessageHelper.NoContent(actionPlanGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to patch Goal [{goalGuid}]");
                return response;
            }

            log.LogInformation($"Attempting to update goal [{goalGuid}]");
            var updatedGoal = await goalsPatchService.UpdateCosmosAsync(patchedGoal, goalGuid);

            if (updatedGoal != null)
            {
                var response = httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(updatedGoal, "id", "GoalId"));
                log.LogInformation($"Response Status Code: [{response.StatusCode}]. Goal updated, sending to service bus [{goalGuid}]");
                await goalsPatchService.SendToServiceBusQueueAsync(updatedGoal, customerGuid, apimUrl);
                return response;
            }
            else
            {
                var response = httpResponseMessageHelper.BadRequest(goalGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Failed to update goal [{goalGuid}]");
                return response;
            }

        }
    }
}
