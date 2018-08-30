using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Annotations;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Ioc;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Goal.PatchGoalHttpTrigger.Function
{
    public static class PatchGoalHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Goal))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goal Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goal does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goal validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing goal record")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Interactions/{interactionId}/ActionPlans/{actionPlanId}/Goals/{goalId}")]HttpRequestMessage req, ILogger log, string customerId, string interactionId, string actionPlanId, string goalId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchGoalHttpTriggerService goalPatchService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Patch Goal C# HTTP trigger function  processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(interactionId, out var interactionGuid))
                return HttpResponseMessageHelper.BadRequest(interactionGuid);

            if (!Guid.TryParse(actionPlanId, out var actionPlanGuid))
                return HttpResponseMessageHelper.BadRequest(actionPlanGuid);

            if (!Guid.TryParse(goalId, out var goalGuid))
                return HttpResponseMessageHelper.BadRequest(goalGuid);

            Models.GoalPatch goalPatchRequest;

            try
            {
                goalPatchRequest = await httpRequestMessageHelper.GetGoalFromRequest<Models.GoalPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (goalPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            goalPatchRequest.LastModifiedBy = touchpointId;

            var errors = validate.ValidateResource(goalPatchRequest, false);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesInteractionExist = resourceHelper.DoesInteractionExist(interactionGuid);

            if (!doesInteractionExist)
                return HttpResponseMessageHelper.NoContent(interactionGuid);

            var doesActionPlanExist = resourceHelper.DoesActionPlanExist(actionPlanGuid);

            if (!doesActionPlanExist)
                return HttpResponseMessageHelper.NoContent(actionPlanGuid);

            var goal = await goalPatchService.GetGoalForCustomerAsync(customerGuid, goalGuid);

            if (goal == null)
                return HttpResponseMessageHelper.NoContent(goalGuid);

            var updatedGoal = await goalPatchService.UpdateAsync(goal, goalPatchRequest);

            if (updatedGoal != null)
                await goalPatchService.SendToServiceBusQueueAsync(updatedGoal, customerGuid, req.RequestUri.AbsoluteUri);

            return updatedGoal == null ?
                HttpResponseMessageHelper.BadRequest(actionPlanGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(updatedGoal));
        }
    }
}