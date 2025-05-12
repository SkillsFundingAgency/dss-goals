using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Goal.GetGoalByIdHttpTrigger.Function
{
    public class GetGoalByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetGoalByIdHttpTriggerService goalsGetByIdService;
        private readonly IHttpRequestHelper httpRequestHelper;
        private readonly IHttpResponseMessageHelper httpResponseMessageHelper;
        private readonly ILogger<GetGoalByIdHttpTrigger> _logger;

        public GetGoalByIdHttpTrigger(IResourceHelper resourceHelper, IHttpRequestHelper _httpRequestHelper, IGetGoalByIdHttpTriggerService _goalsGetByIdService, IHttpResponseMessageHelper _httpResponseMessageHelper, ILogger<GetGoalByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            httpRequestHelper = _httpRequestHelper;
            goalsGetByIdService = _goalsGetByIdService;
            httpResponseMessageHelper = _httpResponseMessageHelper;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Goal), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Goals for the given customer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals/{goalId}")] HttpRequest req, string customerId, string interactionId, string actionPlanId, string goalId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetGoalByIdHttpTrigger));

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }            

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

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}. Correlation GUID: {CorrelationGuid}", touchpointId, correlationGuid);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, correlationGuid);


            _logger.LogInformation("Checking if Interaction exists for Customer. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
            var doesInteractionExist = await _resourceHelper.DoesInteractionExistAndBelongToCustomer(interactionGuid, customerGuid);

            if (!doesInteractionExist)
            {
                _logger.LogWarning("Interaction does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Interaction exists. Customer GUID: {CustomerId}. Interaction GUID: {InteractionGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, correlationGuid);


            _logger.LogInformation("Checking if Action Plan exists for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var doesActionPlanExist = await _resourceHelper.DoesActionPlanExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            if (!doesActionPlanExist)
            {
                _logger.LogWarning("Action Plan does not exist. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Action Plan exists. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);


            _logger.LogInformation("Attempting to get Goal for Customer. Customer GUID: {CustomerId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, actionPlanGuid, correlationGuid);
            var goal = await goalsGetByIdService.GetGoalForCustomerAsync(customerGuid, goalGuid, actionPlanGuid);

            if (goal == null)
            {
                _logger.LogWarning("Goal does not exist. Customer GUID: {CustomerId}. Interaction GUID: {InteractionId}. Action Plan GUID: {ActionPlanGuid}. Correlation GUID: {CorrelationGuid}", customerGuid, interactionGuid, actionPlanGuid, correlationGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetGoalByIdHttpTrigger));
                return new NoContentResult();
            }


            var response = new JsonResult(goal, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };

            _logger.LogInformation("Goal successfully retrieved. Goal GUID: {GoalId}", goal.GoalId);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetGoalByIdHttpTrigger));
            return response;
        }
    }
}