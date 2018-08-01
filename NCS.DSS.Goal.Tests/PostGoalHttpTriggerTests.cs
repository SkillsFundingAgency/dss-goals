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
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests
{
    [TestFixture]
    public class PostGoalHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidInteractionId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostGoalHttpTriggerService _postGoalHttpTriggerService;
        private Models.Goal _goal;

        [SetUp]
        public void Setup()
        {
            _goal = Substitute.For<Models.Goal>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri =
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/" +
                            $"Interactions/aa57e39e-4469-4c79-a9e9-9cb4ef410382/" +
                            $"ActionPlans/d5369b9a-6959-4bd3-92fc-1583e72b7e51/" +
                            $"Goals")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postGoalHttpTriggerService = Substitute.For<IPostGoalHttpTriggerService>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns(new Guid());
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((Guid?)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalHasFailedValidation()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("interaction Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Goal>(), true).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenGoalRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<GoalPatch>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetActionHttpTrigger_ReturnsStatusCodeOk_WhenActionPlanDoesNotExist()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateGoalRecord()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _postGoalHttpTriggerService.CreateAsync(Arg.Any<Models.Goal>()).Returns(Task.FromResult<Models.Goal>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostGoalHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetGoalFromRequest<Models.Goal>(_request).Returns(Task.FromResult(_goal).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _postGoalHttpTriggerService.CreateAsync(Arg.Any<Models.Goal>()).Returns(Task.FromResult(_goal).Result);

            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await PostGoalHttpTrigger.Function.PostGoalHttpTrigger.Run(
                _request, _log, customerId, interactionId, actionPlanId, _resourceHelper, _httpRequestMessageHelper, _validate, _postGoalHttpTriggerService).ConfigureAwait(false);
        }

    }
}