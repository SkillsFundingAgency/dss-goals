using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace NCS.DSS.Goal.Tests.FunctionTests
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

        private Mock<ILogger<PatchGoalHttpTrigger.Function.PatchGoalHttpTrigger>> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IPatchGoalHttpTriggerService> _patchGoalHttpTriggerService;
        private PatchGoalHttpTrigger.Function.PatchGoalHttpTrigger function;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private Models.Goal _goal;
        private GoalPatch _goalPatch;
        private Mock<IValidate> _validate;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _goal = new Models.Goal();
            _goalPatch = new GoalPatch();
            
            _request = new DefaultHttpContext().Request;

            _log = new Mock<ILogger<PatchGoalHttpTrigger.Function.PatchGoalHttpTrigger>>();
            _resourceHelper = new Mock<IResourceHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _patchGoalHttpTriggerService = new Mock<IPatchGoalHttpTriggerService>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();            
            _validate = new Mock<IValidate>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            function = new PatchGoalHttpTrigger.Function.PatchGoalHttpTrigger(_resourceHelper.Object, _httpRequestHelper.Object, _patchGoalHttpTriggerService.Object, _httpResponseMessageHelper, _validate.Object, _dynamicHelper.Object, _log.Object);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenApiurlIsNotProvided()
        {
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>()); ;
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }


        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenGoalIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalHasFailedValidation()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.Setup(x => x.ValidateResource(It.IsAny<GoalPatch>(), false)).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalRequestIsInvalid()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));

            _patchGoalHttpTriggerService.Setup(x => x.GetGoalForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeOk_WhenGoalDoesNotExist()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            _patchGoalHttpTriggerService.Setup(x => x.GetGoalForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalCannotBeFound()
        {
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<GoalPatch>(_request)).Returns(Task.FromResult(_goalPatch));

            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _resourceHelper.Setup(x => x.DoesInteractionExistAndBelongToCustomer(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(true);

            _patchGoalHttpTriggerService.Setup(x => x.GetGoalForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            _patchGoalHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Goal>(null));

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, ValidGoalId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }


        private async Task<IActionResult> RunFunction(string customerId, string interactionId, string actionplanId, string goalId)
        {
            return await function.Run(
                _request,
                customerId,
                interactionId,
                actionplanId,
                goalId).ConfigureAwait(false);
        }
    }
}