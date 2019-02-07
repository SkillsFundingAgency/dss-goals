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
using NCS.DSS.ActionPlan.Models;
using NCS.DSS.ActionPlan.PatchActionPlanHttpTrigger.Service;
using NCS.DSS.ActionPlan.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.ActionPlan.Tests.FunctionTests
{
    [TestFixture]
    public class PatchActionPlanHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPatchActionPlanHttpTriggerService _patchActionPlanHttpTriggerService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Models.ActionPlan _actionPlan;
        private ActionPlanPatch _actionPlanPatch;

        [SetUp]
        public void Setup()
        {
            _actionPlan = Substitute.For<Models.ActionPlan>();
            _actionPlanPatch = Substitute.For<ActionPlanPatch>();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _patchActionPlanHttpTriggerService = Substitute.For<IPatchActionPlanHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
            _httpRequestHelper.GetDssApimUrl(_request).Returns("http://localhost:");


        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.GetDssTouchpointId(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.GetDssApimUrl(_request).Returns((string)null);

            _httpResponseMessageHelper.BadRequest().Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }


        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenSessionIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionPlanHasFailedValidation()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<ActionPlanPatch>()).Returns(validationResults);

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<List<ValidationResult>>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));
            
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenActionPlanRequestIsInvalid()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Throws(new JsonException());

            _httpResponseMessageHelper
                .UnprocessableEntity(Arg.Any<JsonException>()).Returns(x => new HttpResponseMessage((HttpStatusCode)422));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchActionPlanHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchActionPlanHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateActionPlanRecord()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchActionPlanHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult("actionplan").Result);

            _patchActionPlanHttpTriggerService.UpdateAsync(Arg.Any<string>(), Arg.Any<ActionPlanPatch>(),Arg.Any<Guid>()).Returns(Task.FromResult<Models.ActionPlan>(null).Result);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanCannotBeFound()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchActionPlanHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            _patchActionPlanHttpTriggerService.UpdateAsync(Arg.Any<string>(), Arg.Any<ActionPlanPatch>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.ActionPlan>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchActionPlanHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestHelper.GetResourceFromRequest<ActionPlanPatch>(_request).Returns(Task.FromResult(_actionPlanPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _patchActionPlanHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult("actionPlan").Result);

            _patchActionPlanHttpTriggerService.UpdateAsync(Arg.Any<string>(), Arg.Any<ActionPlanPatch>(), Arg.Any<Guid>()).Returns(Task.FromResult(_actionPlan).Result);

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string sessionId, string actionPlanId)
        {
            return await PatchActionPlanHttpTrigger.Function.PatchActionPlanHttpTrigger.Run(
                _request,
                _log,
                customerId,
                interactionId,
                sessionId,
                actionPlanId,
                _resourceHelper,
                _validate,
                _patchActionPlanHttpTriggerService,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }
    }
}