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
using NCS.DSS.ActionPlan.Cosmos.Helper;
using NCS.DSS.ActionPlan.PostActionPlanHttpTrigger.Service;
using NCS.DSS.ActionPlan.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.ActionPlan.Tests.FunctionTests
{
    [TestFixture]
    public class PostActionPlanHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPostActionPlanHttpTriggerService _postActionPlanHttpTriggerService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Models.ActionPlan _actionPlan;

        [SetUp]
        public void Setup()
        {
            _actionPlan = Substitute.For<Models.ActionPlan>();
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger>(); _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _postActionPlanHttpTriggerService = Substitute.For<IPostActionPlanHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:");
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.GetDssApimUrl(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenSessionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionPlanHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.ActionPlan>()).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionPlanRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateActionPlanRecord()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postActionPlanHttpTriggerService.CreateAsync(Arg.Any<Models.ActionPlan>()).Returns(Task.FromResult<Models.ActionPlan>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenRequestIsNotValid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postActionPlanHttpTriggerService.CreateAsync(Arg.Any<Models.ActionPlan>()).Returns(Task.FromResult<Models.ActionPlan>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostActionPlanHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestHelper.GetResourceFromRequest<Models.ActionPlan>(_request).Returns(Task.FromResult(_actionPlan).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _postActionPlanHttpTriggerService.CreateAsync(Arg.Any<Models.ActionPlan>()).Returns(Task.FromResult(_actionPlan).Result);

            _httpResponseMessageHelper
                .Created(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.Created));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string sessionId)
        {
            return await PostActionPlanHttpTrigger.Function.PostActionPlanHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                sessionId,
                _resourceHelper,
                _validate,
                _postActionPlanHttpTriggerService,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }
    }
}