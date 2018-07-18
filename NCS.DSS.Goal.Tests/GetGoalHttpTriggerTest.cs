using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests
{
    [TestFixture]
    public class GetGoalHttpTriggerTest
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private const string ValidActionPlanId = "d5369b9a-6959-4bd3-92fc-1583e72b7e51";
        private const string ValidInteractionId = "233d861d-05ca-496e-aee1-6bd47ac13cb2";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetGoalHttpTriggerService _getGoalHttpTriggerService;
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
                                $"Interactions/233d861d-05ca-496e-aee1-6bd47ac13cb2/" +
                                $"ActionPlans/d5369b9a-6959-4bd3-92fc-1583e72b7e51/" +
                                $"Goals")
            };
            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getGoalHttpTriggerService = Substitute.For<IGetGoalHttpTriggerService>();
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenInteractionIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeBadRequest_WhenActionPlanIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }
        
        
        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenInteractionDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }


        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeNoContent_WhenActionPlanDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(false);

            _getGoalHttpTriggerService.GetGoalsAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Goal>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeOk_WhenGoalDoesNotExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            _getGoalHttpTriggerService.GetGoalsAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Goal>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetGoalHttpTrigger_ReturnsStatusCodeOk_WhenGoalExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesInteractionExist(Arg.Any<Guid>()).Returns(true);
            _resourceHelper.DoesActionPlanExist(Arg.Any<Guid>()).Returns(true);

            var goals = new List<Models.Goal>();
            _getGoalHttpTriggerService.GetGoalsAsync(Arg.Any<Guid>()).Returns(Task.FromResult(goals).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidInteractionId, ValidActionPlanId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string interactionId, string actionPlanId)
        {
            return await GetGoalHttpTrigger.Function.GetGoalHttpTrigger.Run(
                _request, _log, customerId, interactionId, actionPlanId, _resourceHelper, _getGoalHttpTriggerService).ConfigureAwait(false);
        }

    }
}