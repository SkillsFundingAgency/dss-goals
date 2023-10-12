using System;
using System.ComponentModel.DataAnnotations;
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
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;

namespace NCS.DSS.Goal.GetGoalHttpTrigger.Function
{
    public class GetGoalHttpTrigger
    {
        private IResourceHelper resourceHelper;
        private readonly IGetGoalHttpTriggerService goalsGetService;
        private ILoggerHelper loggerHelper;
        private IHttpRequestHelper httpRequestHelper;
        private IHttpResponseMessageHelper httpResponseMessageHelper;
        private IJsonHelper jsonHelper;
        public GetGoalHttpTrigger(IResourceHelper _resourceHelper, IHttpRequestHelper _httpRequestHelper, IGetGoalHttpTriggerService _goalsGetService, IHttpResponseMessageHelper _httpResponseMessageHelper, IJsonHelper _jsonHelper, ILoggerHelper _loggerHelper)
        {
            resourceHelper = _resourceHelper;
            httpRequestHelper = _httpRequestHelper;
            goalsGetService = _goalsGetService;
            httpResponseMessageHelper = _httpResponseMessageHelper;
            jsonHelper = _jsonHelper;
            loggerHelper = _loggerHelper;
        }

        [FunctionName("Get")]
        [ProducesResponseType(typeof(Models.Goal), (int)HttpStatusCode.OK)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return all Goals for the given Interactions.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionPlanId)
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

            log.LogInformation($"Get Action Plan C# HTTP trigger function  processed a request. By Touchpoint: [{touchpointId}]");

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
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. Unable to parse 'actionplanId' to a Guid: [{actionPlanGuid}]");
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

            log.LogInformation($"Attempting to get goals for customer [{customerGuid}]");
            var goals = await goalsGetService.GetGoalsAsync(customerGuid, actionPlanGuid);

            if (goals == null)
            {
                var response = httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: [{response.StatusCode}]. No goals found for customer [{customerGuid}]");
                return response;
            }
            else
            {
                var response = httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectsAndRenameIdProperty(goals, "id", "GoalId"));
                log.LogInformation($"Response Status Code: [{response.StatusCode}]. Goals found for customer [{customerGuid}]");
                return response;
            }

        }
    }
}