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
        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchGoalHttpTriggerService _goalsPatchService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PatchGoalHttpTrigger> _logger;

        public PatchGoalHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestHelper, IPatchGoalHttpTriggerService goalsPatchService, IHttpResponseMessageHelper httpResponseMessageHelper, IValidate validate, IDynamicHelper dynamicHelper, ILogger<PatchGoalHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _goalsPatchService = goalsPatchService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _validate = validate;
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchGoalHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header.");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogWarning("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);                
            }

            _logger.LogInformation("Header validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(interactionId, out var interactionGuid))
            {
                _logger.LogWarning("Unable to parse 'interactionId' to a GUID. Interaction ID: {InteractionId}", interactionId);
                return new BadRequestObjectResult(interactionGuid);
            }

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
            {
                _logger.LogWarning("Unable to parse 'actionPlanId' to a GUID. Action Plan ID: {ActionplanId}", actionPlanId);
                return new BadRequestObjectResult(actionPlanGuid);
            }

            if (!Guid.TryParse(goalId, out var goalGuid))
            {
                _logger.LogWarning("Unable to parse 'goalId' to a GUID. GoalId ID: {GoalId}", goalId);
                return new BadRequestObjectResult(goalGuid);
            }

            GoalPatch goalPatchRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                goalPatchRequest = await _httpRequestHelper.GetResourceFromRequest<GoalPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {goalPatchRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(goalPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (goalPatchRequest == null)
            {
                _logger.LogWarning("{goalPatchRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(goalPatchRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to set IDs for Goal PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);
            goalPatchRequest.SetIds(touchpointId, subcontractorId);
            _logger.LogInformation("IDs successfully set for Goal PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);

            _logger.LogInformation("Attempting to validate {goalPatchRequest} object", nameof(goalPatchRequest));
            var errors = _validate.ValidateResource(goalPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Failed to validate {goalPatchRequest} object", nameof(goalPatchRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {goalPatchRequest} object", nameof(goalPatchRequest));


            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Check if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation("Checking if Interaction exists for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);


            _logger.LogInformation("Checking if action plan exists for customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var doesActionPlanExist = await _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogWarning("Action plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Action plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);


            _logger.LogInformation("Attempting to get goal for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var goalForCustomer = await _goalsPatchService.GetGoalForCustomerAsync(customerGuid, goalGuid, actionPlanGuid);

            if (goalForCustomer == null)
            {
                _logger.LogWarning("Goal not found. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Goal successfully retrieved. Goal GUID: {GoalId}", goalGuid);

            _logger.LogInformation("Attempting to PATCH Goal resource.");
            var patchedGoal = _goalsPatchService.PatchResource(goalForCustomer, goalPatchRequest);

            if (patchedGoal == null)
            {
                _logger.LogWarning("Failed to PATCH Goal resource.");
                return new NoContentResult();
            }

            _logger.LogInformation("Attempting to update Goal in Cosmos DB. Goal GUID: {GoalId}", goalGuid);
            var updatedGoal = await _goalsPatchService.UpdateCosmosAsync(patchedGoal, goalGuid);

            if (updatedGoal == null)
            {
                _logger.LogWarning("Goal update request unsuccessful. Goal GUID: {GoalId}", goalGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchGoalHttpTrigger));
                return new BadRequestObjectResult(goalGuid);
            }

            _logger.LogInformation("Goal updated successfully in Cosmos DB. Goal GUID: {GoalId}", goalGuid);

            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Goal GUID: {GoalId}", goalId);
            await _goalsPatchService.SendToServiceBusQueueAsync(updatedGoal, customerGuid, apimUrl);
            _logger.LogInformation("Successfully sent message to Service Bus. Goal GUID: {GoalId}", goalId);

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchGoalHttpTrigger));
            var response = new JsonResult(updatedGoal, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
            return response;
        }
    }
}
