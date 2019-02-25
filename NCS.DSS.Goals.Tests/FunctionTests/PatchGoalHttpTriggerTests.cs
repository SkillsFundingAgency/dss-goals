using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goals.Cosmos.Helper;
using NCS.DSS.Goals.Models;
using NCS.DSS.Goals.PatchGoalsHttpTrigger.Service;
using NCS.DSS.Goals.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Goals.Tests.FunctionTests
{
    [TestFixture]
    public class PatchGoalHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidGoalId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPatchGoalsHttpTriggerService _patchGoalHttpTriggerService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Models.Goal _Goal;
        private GoalPatch _GoalPatch;

        [SetUp]
        public void Setup()
        {
            _Goal = Substitute.For<Models.Goal>();
            _GoalPatch = Substitute.For<GoalPatch>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _patchGoalHttpTriggerService = Substitute.For<IPatchGoalsHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:");


        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.GetDssApimUrl(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

               
        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenGoalIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<GoalPatch>(), false).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));
            
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeOk_WhenGoalDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalCannotBeFound()
        {
            _httpRequestHelper.GetResourceFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_GoalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _patchGoalHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        
        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionplanId, string goalId)
        {
            return await PatchGoalsHttpTrigger.Function.PatchGoalsHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                actionplanId,
                goalId,
                _resourceHelper,
                _patchGoalHttpTriggerService,
                _validate,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }
    }
}