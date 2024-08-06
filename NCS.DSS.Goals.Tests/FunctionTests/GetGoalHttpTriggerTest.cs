using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace NCS.DSS.Goal.Tests.FunctionTests
{
    [TestFixture]
    public class GetGoalHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private Mock<ILogger<GetGoalHttpTrigger.Function.GetGoalHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IGetGoalHttpTriggerService> _getGoalHttpTriggerService;
        private Models.Goal _goal;
        private GetGoalHttpTrigger.Function.GetGoalHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<ILoggerHelper> _loggerHelper;


        [SetUp]
        public void Setup()
        {
            _goal = new Models.Goal();


            _loggerHelper = new Mock<ILoggerHelper>();
            _request = new DefaultHttpContext().Request;

            _log = new Mock<ILogger<GetGoalHttpTrigger.Function.GetGoalHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _getGoalHttpTriggerService = new Mock<IGetGoalHttpTriggerService>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            function = new GetGoalHttpTrigger.Function.GetGoalHttpTrigger(_resourceHelper.Object, _httpRequestHelper.Object, _getGoalHttpTriggerService.Object, _httpResponseMessageHelper, _jsonHelper, _loggerHelper.Object, _log.Object);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenDssCorrelationIdIsInvalid()
        {
            _httpRequestHelper.Setup(x => x.GetDssCorrelationId(_request)).Returns(InValidId);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }
        
        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _getGoalHttpTriggerService.Setup(x => x.GetGoalsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult <List<Models.Goal>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeOk_WhenGoalExists()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            var listOfGoales = new List<Models.Goal>();
            _getGoalHttpTriggerService.Setup(x => x.GetGoalsAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult(listOfGoales));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionplanId)
        {
            return await function.Run(
                _request,
                customerId,
                interactionId,
                actionplanId).ConfigureAwait(false);
        }
    }
}