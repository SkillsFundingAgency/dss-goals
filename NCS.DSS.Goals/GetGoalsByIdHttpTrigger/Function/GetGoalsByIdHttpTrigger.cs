using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goals.Cosmos.Helper;
using NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Service;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Mvc;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;

namespace NCS.DSS.Goals.GetGoalsByIdHttpTrigger.Function
{
    public static class GetGoalsByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Goal), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Goals found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Goals does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve an individual Goals for the given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Interactions/{interactionId}/sessions/{sessionId}/actionplans/{actionplanId}/Goals/{GoalId}")]HttpRequest req, ILogger log, string customerId, string interactionId, string actionplanId, string GoalId, string sessionId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IGetGoalsByIdHttpTriggerService GoalsGetService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {

            Models.Goal testGoal = new Models.Goal
            {
                GoalId = Guid.Parse("01cda95f-9a4e-41fa-aee3-10c3e55ad94a"),
                CustomerId = Guid.Parse("cca80ff0-23c2-45ff-b82f-47947ec8b783"),
                SessionId = Guid.Parse("f0ec575f-5782-4cca-8eac-62644ae59786"),
                ActionPlanId = Guid.Parse("af171e23-fbd0-41d0-bd66-886ae60e9b21"),
                DateGoalAchieved = DateTime.Parse("01/06/2018"),
                DateGoalCaptured = DateTime.Parse("01/02/2018"),
                DateGoalShouldBeCompletedBy = DateTime.Parse("01/05/2018"),
                LastModifiedDate = DateTime.Parse("01/08/2018"),
                GoalStatus = ReferenceData.GoalStatus.Achieved,
                GoalType = ReferenceData.GoalType.Work,
                GoalSummary = "Summary of Goal in Text form",
                LastModifiedBy = "Example Last Modified By",
                LastModifiedTouchpointId = "0000000010"
            };

            return httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(testGoal, "id", "GoalId"));


            //var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            //if (string.IsNullOrEmpty(touchpointId))
            //{
            //    log.LogInformation("Unable to locate 'APIM-TouchpointId' in request header.");
            //    return httpResponseMessageHelper.BadRequest();
            //}

            //log.LogInformation("Get Goals By Id C# HTTP trigger function  processed a request. " + touchpointId);

            //if (!Guid.TryParse(customerId, out var customerGuid))
            //    return httpResponseMessageHelper.BadRequest(customerGuid);

            //if (!Guid.TryParse(interactionId, out var interactionGuid))
            //    return httpResponseMessageHelper.BadRequest(interactionGuid);

            //if (!Guid.TryParse(actionplanId, out var actionPlanGuid))
            //    return httpResponseMessageHelper.BadRequest(actionPlanGuid);

            //if (!Guid.TryParse(OutcomeId, out var GoalsGuid))
            //    return httpResponseMessageHelper.BadRequest(GoalsGuid);

            ////Check customer
            //var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            //if (!doesCustomerExist)
            //    return httpResponseMessageHelper.NoContent(customerGuid);

            //var doesInteractionExist = resourceHelper.DoesInteractionResourceExistAndBelongToCustomer(interactionGuid, customerGuid);

            //if (!doesInteractionExist)
            //    return httpResponseMessageHelper.NoContent(interactionGuid);

            //var doesActionPlanExist = resourceHelper.DoesActionPlanResourceExistAndBelongToCustomer(actionPlanGuid, interactionGuid, customerGuid);

            //if (!doesActionPlanExist)
            //    return httpResponseMessageHelper.NoContent(actionPlanGuid);

            //var Goals = await GoalsGetService.GetGoalsForCustomerAsync(customerGuid, interactionGuid, actionPlanGuid, GoalsGuid);

            //return Goals == null ?
            //    httpResponseMessageHelper.NoContent(GoalsGuid) :
            //    httpResponseMessageHelper.Ok(jsonHelper.SerializeObjectAndRenameIdProperty(Goals, "id", "OutcomeId"));

        }
    }
}