using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Goal.PostGoalHttpTrigger.Function
{
    public class PostGoalHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IPostGoalHttpTriggerService _goalsPostService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private readonly ILogger<PostGoalHttpTrigger> _logger;

        public PostGoalHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper httpRequestHelper, IPostGoalHttpTriggerService goalsPostService, IHttpResponseMessageHelper httpResponseMessageHelper, IValidate validate, IDynamicHelper dynamicHelper, ILogger<PostGoalHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _httpRequestHelper = httpRequestHelper;
            _goalsPostService = goalsPostService;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
            _logger = logger;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Goal), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Goals Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goals validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Goals for a customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals")] HttpRequest req, string customerId, string interactionId, string actionPlanId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostGoalHttpTrigger));

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

            Models.Goal goalRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                goalRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Goal>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {goalRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(goalRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (goalRequest == null)
            {
                _logger.LogWarning("{goalRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(goalRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to set IDs for Goal. Correlation GUID: {CorrelationGuid}", correlationGuid);
            goalRequest.SetIds(customerGuid, actionPlanGuid, touchpointId, subcontractorId);
            _logger.LogInformation("IDs successfully set for Goal. Correlation GUID: {CorrelationGuid}", correlationGuid);

            _logger.LogInformation("Attempting to validate {goalRequest} object", nameof(goalRequest));
            var errors = _validate.ValidateResource(goalRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Failed to validate {goalRequest} object", nameof(goalRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {goalRequest} object", nameof(goalRequest));


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


            _logger.LogInformation("Attempting to create Goal for customer in Cosmos DB. Customer GUID: {CustomerId}", customerGuid);
            var goal = await _goalsPostService.CreateAsync(goalRequest);

            if (goal == null)
            {
                _logger.LogWarning("Failed to create Goal for customer. Customer GUID: {CustomerId}", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostGoalHttpTrigger));
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Goal created successfully in Cosmos DB. Goal GUID: {GoalId}", goal.GoalId);

            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Goal GUID: {GoalId}", goal.GoalId);
            await _goalsPostService.SendToServiceBusQueueAsync(goal, apimUrl);
            _logger.LogInformation("Successfully sent message to Service Bus. Goal GUID: {GoalId}", goal.GoalId);

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostGoalHttpTrigger));
            var response = new JsonResult(goal, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
            return response;
        }
    }
}