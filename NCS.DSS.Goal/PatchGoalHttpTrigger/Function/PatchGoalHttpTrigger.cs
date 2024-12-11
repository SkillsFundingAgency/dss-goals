using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Function
{
    public class PatchGoalHttpTrigger
    {
        private readonly IResourceHelper resourceHelper;
        private readonly IPatchGoalHttpTriggerService goalsPatchService;
        private readonly IHttpRequestHelper httpRequestHelper;
        private readonly IHttpResponseMessageHelper httpResponseMessageHelper;
        private readonly IValidate validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PatchGoalHttpTrigger> _logger;

        public PatchGoalHttpTrigger(IResourceHelper _resourceHelper, IHttpRequestHelper _httpRequestHelper, IPatchGoalHttpTriggerService _goalsPatchService, IHttpResponseMessageHelper _httpResponseMessageHelper, IValidate _validate, IDynamicHelper dynamicHelper, ILogger<PatchGoalHttpTrigger> logger)
        {
            resourceHelper = _resourceHelper;
            httpRequestHelper = _httpRequestHelper;
            goalsPatchService = _goalsPatchService;
            httpResponseMessageHelper = _httpResponseMessageHelper;
            validate = _validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Goal), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goals validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update a customers Goals record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals/{goalId}")] HttpRequest req, string customerId, string interactionId, string actionPlanId, string goalId)
        {

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            _logger.LogInformation($"DssCorrelationId: [{correlationGuid}]");

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'TouchpointId' in request header");
                return response;
            }

            var apimUrl = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = new BadRequestObjectResult(HttpStatusCode.BadRequest);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to locate 'apimurl' in request header");
                return response;
            }

            var subcontractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
                _logger.LogInformation($"Unable to locate 'SubcontractorId' in request header");

            _logger.LogInformation($"Post Actions C# HTTP trigger function  processed a request. By Touchpoint: [{touchpointId}]");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'customerId' to a Guid: [{customerId}]");
                return response;
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                var response = new BadRequestObjectResult(interactionGuid);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'interactionId' to a Guid: [{interactionId}]");
                return response;
            }

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
            {
                var response = new BadRequestObjectResult(actionPlanGuid);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'actionPlanId' to a Guid: [{actionPlanId}]");
                return response;
            }

            if (!Guid.TryParse(goalId, out var goalGuid))
            {
                var response = new BadRequestObjectResult(goalGuid);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'GoalId' to a Guid: [{goalId}]");
                return response;
            }

            GoalPatch goalPatchRequest;

            try
            {
                _logger.LogInformation($"Attempt to get resource from body of the request");
                goalPatchRequest = await httpRequestHelper.GetResourceFromRequest<GoalPatch>(req);
            }
            catch (Exception ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
                _logger.LogError($"Response Status Code: [{response.StatusCode}]. Unable to retrieve body from req", ex);
                return response;
            }

            if (goalPatchRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Goal patch request is null");
                return response;
            }

            _logger.LogInformation($"Attempt to set id's for Goal");
            goalPatchRequest.SetIds(touchpointId, subcontractorId);

            _logger.LogInformation($"Attempt to validate resource");
            var errors = validate.ValidateResource(goalPatchRequest, false);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Validation errors: [{errors.FirstOrDefault().ErrorMessage}]");
                return response;
            }

            _logger.LogInformation($"Attempting to see if customer exists [{customerGuid}]");
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer does not exist [{customerGuid}]");
                return response;
            }

            _logger.LogInformation($"Attempting to see if this is a read only customer [{customerGuid}]");
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Customer is read only [{customerGuid}]");
                return response;
            }

            _logger.LogInformation($"Attempting to get Interaction [{interactionGuid}] for customer [{customerGuid}]");
            var doesInteractionExist = await resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Interaction does not exist [{interactionGuid}]");
                return response;
            }

            var doesActionPlanExistAndBelongToCustomer = await resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExistAndBelongToCustomer)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Action Plan does not exist [{actionPlanGuid}]");
                return response;
            }

            _logger.LogInformation($"Attempting to get goal [{goalGuid}] for customer [{customerGuid}]");
            var goalForCustomer = await goalsPatchService.GetGoalForCustomerAsync(customerGuid, goalGuid, actionPlanGuid);

            if (goalForCustomer == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Goal does not exist [{goalGuid}]");
                return response;
            }

            var patchedGoal = goalsPatchService.PatchResource(goalForCustomer, goalPatchRequest);

            if (patchedGoal == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to patch Goal [{goalGuid}]");
                return response;
            }

            _logger.LogInformation($"Attempting to update goal [{goalGuid}]");
            var updatedGoal = await goalsPatchService.UpdateCosmosAsync(patchedGoal, goalGuid);

            if (updatedGoal != null)
            {
                var response = new JsonResult(updatedGoal, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
                _logger.LogInformation($"Response Status Code: [{response.StatusCode}]. Goal updated, sending to service bus [{goalGuid}]");
                await goalsPatchService.SendToServiceBusQueueAsync(updatedGoal, customerGuid, apimUrl);
                return response;
            }
            else
            {
                var response = new BadRequestObjectResult(goalGuid);
                _logger.LogWarning($"Response Status Code: [{response.StatusCode}]. Failed to update goal [{goalGuid}]");
                return response;
            }

        }
    }
}
