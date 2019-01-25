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
using NCS.DSS.Goals.PostGoalsHttpTrigger.Service;
using NCS.DSS.Goals.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Goals.PostGoalsHttpTrigger.Function
{
    public static class PostGoalsHttpTrigger
    {
        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Goal),200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Goals Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Goals validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new Goals for a customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Goals")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IPostGoalsHttpTriggerService GoalsPostService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {


            return httpResponseMessageHelper.Ok("Created Goals successfully!");



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

            //log.LogInformation("Post Action Plan C# HTTP trigger function processed a request. " + touchpointId);

            //if (!Guid.TryParse(customerId, out var customerGuid))
            //    return httpResponseMessageHelper.BadRequest(customerGuid);

            //if (!Guid.TryParse(interactionId, out var interactionGuid))
            //    return httpResponseMessageHelper.BadRequest(interactionGuid);

            //if (!Guid.TryParse(actionplanId, out var actionplanGuid))
            //    return httpResponseMessageHelper.BadRequest(actionplanGuid);

            //Models.Goal GoalsRequest;

            //try
            //{
            //    GoalsRequest = await httpRequestHelper.GetResourceFromRequest<Models.Goal>(req);
            //}
            //catch (JsonException ex)
            //{
            //    return httpResponseMessageHelper.UnprocessableEntity(ex);
            //}

            //if (GoalsRequest == null)
            //    return httpResponseMessageHelper.UnprocessableEntity(req);

            //GoalsRequest.SetIds(customerGuid, actionplanGuid, touchpointId, subcontractorid);

            //var errors = validate.ValidateResource(GoalsRequest);

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

            //var Goals = await GoalsPostService.CreateAsync(GoalsRequest);

            //if (Goals != null)
            //    await GoalsPostService.SendToServiceBusQueueAsync(Goals, ApimURL);

            //return Goals == null
            //    ? httpResponseMessageHelper.BadRequest(customerGuid)
            //    : httpResponseMessageHelper.Created(jsonHelper.SerializeObjectAndRenameIdProperty(Goals, "id", "OutcomeId"));

        }
    }
}