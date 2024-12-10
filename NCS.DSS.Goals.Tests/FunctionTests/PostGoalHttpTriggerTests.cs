using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Tests.FunctionTests
{
    [TestFixture]
    public class PostGoalHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidDssCorrelationId = "452d8e8c-2516-4a6b-9fc1-c85e578ac066";
        private const string ValidActionPlanId = "b8592ff8-af97-49ad-9fb2-e5c3c717fd85";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private Mock<ILogger<PostGoalHttpTrigger.Function.PostGoalHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IPostGoalHttpTriggerService> _postGoalHttpTriggerService;
        private PostGoalHttpTrigger.Function.PostGoalHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;        
        private Models.Goal _goal;
        private Mock<IValidate> _validate;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _goal = new Models.Goal();
                        
            _request = new DefaultHttpContext().Request;
            _log = new Mock<ILogger<PostGoalHttpTrigger.Function.PostGoalHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _postGoalHttpTriggerService = new Mock<IPostGoalHttpTriggerService>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();            
            _validate = new Mock<IValidate>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            function = new PostGoalHttpTrigger.Function.PostGoalHttpTrigger(_resourceHelper.Object, _httpRequestHelper.Object, _postGoalHttpTriggerService.Object, _httpResponseMessageHelper, _validate.Object, _dynamicHelper.Object, _log.Object);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalHasFailedValidation()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Returns(Task.FromResult(_goal));

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.Setup(x => x.ValidateResource(It.IsAny<Models.Goal>(), true)).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalRequestIsInvalid()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Returns(Task.FromResult(_goal));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Returns(Task.FromResult(_goal));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateGoalRecord()
        {
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Returns(Task.FromResult(_goal));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            _postGoalHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Goal>())).Returns(Task.FromResult<Models.Goal>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Goal>(_request)).Returns(Task.FromResult(_goal));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _resourceHelper.Setup(x => x.DoesActionPlanExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);
            _postGoalHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Goal>())).Returns(Task.FromResult(_goal));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);
            var responseResult = result as JsonResult;
            //Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
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