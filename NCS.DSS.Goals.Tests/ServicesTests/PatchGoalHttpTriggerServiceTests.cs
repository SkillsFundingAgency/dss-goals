using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServicesTests
{
    [TestFixture]
    public class PatchGoalHttpTriggerServiceTests
    {
        private IPatchGoalHttpTriggerService _goalHttpTriggerService;
        private IGoalPatchService _goalPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Goal _goal;
        private GoalPatch _goalPatch;
        private readonly Guid _goalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _goalPatchService = Substitute.For<IGoalPatchService>();
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _goalHttpTriggerService = Substitute.For<PatchGoalHttpTriggerService>(_goalPatchService, _documentDbProvider);
            _goalPatch = Substitute.For<GoalPatch>();
            _goal = Substitute.For<Models.Goal>();

            _json = JsonConvert.SerializeObject(_goalPatch);
            _goalPatchService.Patch(_json, _goalPatch).Returns(_goal.ToString());
        }

        [Test]
        public void PatchgoalHttpTriggerServiceTests_PatchResource_ReturnsNullWhenGoalJsonIsNullOrEmpty()
        {
            // Act
            var result = _goalHttpTriggerService.PatchResource(null, Arg.Any<GoalPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void PatchgoalHttpTriggerServiceTests_PatchResource_ReturnsNullWhenGoalPatchIsNullOrEmpty()
        {
            // Act
            var result = _goalHttpTriggerService.PatchResource(Arg.Any<string>(), null);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalIsNullOrEmpty()
        {
            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(null, _goalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalPatchServicePatchJsonIsNullOrEmpty()
        {
            _goalPatchService.Patch(Arg.Any<string>(), Arg.Any<GoalPatch>()).ReturnsNull();

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), _goalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateGoalAsync(Arg.Any<string>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), _goalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateGoalAsync(Arg.Any<Models.Goal>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(_goal.ToString(), _goalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.OK, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.UpdateGoalAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _goalHttpTriggerService.UpdateCosmosAsync(_goal.ToString(), _goalId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Goal>(result);

        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_GetgoalForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetGoalForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _goalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchgoalHttpTriggerServiceTests_GetgoalForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetGoalForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _goalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}