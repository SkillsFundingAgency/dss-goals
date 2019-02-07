using System;
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
using NCS.DSS.ActionPlan.GetActionPlanByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.ActionPlan.Tests.FunctionTests
{
    [TestFixture]
    public class GetActionPlanByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidSessionId = "58b43e3f-4a50-4900-9c82-a14682ee90fa";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private ILogger _log;
        private HttpRequest _request;
        private IResourceHelper _resourceHelper;
        private IGetActionPlanByIdHttpTriggerService _getActionPlanByIdHttpTriggerService;
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

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _loggerHelper = Substitute.For<ILoggerHelper>();
            _httpRequestHelper = Substitute.For<IHttpRequestHelper>();
            _httpResponseMessageHelper = Substitute.For<IHttpResponseMessageHelper>();
            _jsonHelper = Substitute.For<IJsonHelper>();
            _getActionPlanByIdHttpTriggerService = Substitute.For<IGetActionPlanByIdHttpTriggerService>();

            _httpRequestHelper.GetDssCorrelationId(_request).Returns(ValidDssCorrelationId);
            _httpRequestHelper.GetDssTouchpointId(_request).Returns("0000000001");
        }

        [Test]
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenDssCorrelationIdIsInvalid()
        {
            _httpRequestHelper.GetDssCorrelationId(_request).Returns(InValidId);

            _httpResponseMessageHelper
                .BadRequest(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.BadRequest));

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenSessionIdIsInvalid()
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
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
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getActionPlanByIdHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.ActionPlan>(null).Result);

            _httpResponseMessageHelper
                .NoContent(Arg.Any<Guid>()).Returns(x => new HttpResponseMessage(HttpStatusCode.NoContent));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionPlanByIdHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesSessionExistAndBelongToCustomer(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(true);

            _getActionPlanByIdHttpTriggerService.GetActionPlanForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_actionPlan).Result);

            _httpResponseMessageHelper
                .Ok(Arg.Any<string>()).Returns(x => new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidSessionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string sessionId, string actionPlanId)
        {
            return await GetActionPlanByIdHttpTrigger.Function.GetActionPlanByIdHttpTrigger.Run(
                _request, 
                _log, 
                customerId, 
                interactionId,
                sessionId,
                actionPlanId, 
                _resourceHelper,
                _getActionPlanByIdHttpTriggerService,
                _loggerHelper,
                _httpRequestHelper,
                _httpResponseMessageHelper,
                _jsonHelper).ConfigureAwait(false);
        }
    }
}