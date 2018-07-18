using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests
{
    [TestFixture]
    public class PatchGoalHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidGoalId = "233d861d-05ca-496e-aee1-6bd47ac13cb2";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPatchGoalHttpTriggerService _patchGoalHttpTriggerService;
        private Models.Goal _goal;
        private GoalPatch _goalPatch;

        [SetUp]
        public void Setup()
        {
            _goal = Substitute.For<Models.Goal>();
            _goalPatch = Substitute.For<GoalPatch>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Interactions/aa57e39e-4469-4c79-a9e9-9cb4ef410382/" +
                            $"ActionPlans/d5369b9a-6959-4bd3-92fc-1583e72b7e51/" +
                            $"Goals/d5369b9a-6959-4bd3-92fc-1583e72b7e51")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _patchGoalHttpTriggerService = Substitute.For<IPatchGoalHttpTriggerService>();
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId , ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenGoalIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalHasFailedValidation()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<GoalPatch>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalRequestIsInvalid()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(Arg.Any<GoalPatch>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenGoalDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenGoalPlanDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(false);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateActionRecord()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_goal).Result);

            _patchGoalHttpTriggerService.UpdateAsync(Arg.Any<Models.Goal>(), Arg.Any<GoalPatch>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchGoalHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Returns(Task.FromResult(_goalPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _patchGoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_goal).Result);

            _patchGoalHttpTriggerService.UpdateAsync(Arg.Any<Models.Goal>(), Arg.Any<GoalPatch>()).Returns(Task.FromResult(_goal).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidGoalId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string InteractionId, string actionPlanId, string GoalId)
        {
            return await PatchGoalHttpTrigger.Function.PatchGoalHttpTrigger.Run(
                _request, _log, customerId, InteractionId, actionPlanId, GoalId, _resourceHelper, _httpRequestMessageHelper, _validate, _patchGoalHttpTriggerService).ConfigureAwait(false);
        }

    }
}