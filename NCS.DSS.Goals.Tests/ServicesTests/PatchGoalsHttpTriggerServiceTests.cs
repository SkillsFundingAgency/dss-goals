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
using NCS.DSS.Goal.PatchGoalsHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServiceTests
{
    [TestFixture]
    public class PatchGoalHttpTriggerServiceTests
    {
        private IPatchGoalsHttpTriggerService _GoalHttpTriggerService;
        private IGoalsPatchService _GoalPatchService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private GoalPatch _GoalPatch;
        private readonly Guid _GoalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _GoalPatchService = Substitute.For<IGoalsPatchService>();
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _GoalHttpTriggerService = Substitute.For<PatchGoalsHttpTriggerService>(_GoalPatchService, _documentDbProvider);
            _GoalPatch = Substitute.For<GoalPatch>();
            _json = JsonConvert.SerializeObject(_GoalPatch);
            _GoalPatchService.Patch(_json, _GoalPatch).Returns(_json);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalJsonIsNullOrEmpty()
        {
            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(null, Arg.Any<GoalPatch>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalPatchIsNullOrEmpty()
        {
            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(Arg.Any<string>(), null, Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenGoalPatchServicePatchJsonIsNullOrEmpty()
        {
            _GoalPatchService.Patch(Arg.Any<string>(), Arg.Any<GoalPatch>()).ReturnsNull();

            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(_json, _GoalPatch, _GoalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeUpdated()
        {
            _documentDbProvider.UpdateGoalsAsync(Arg.Any<string>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(_json, _GoalPatch, _GoalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsNullWhenResourceCannotBeFound()
        {
            _documentDbProvider.CreateGoalsAsync(Arg.Any<Models.Goal>()).Returns(Task.FromResult(new ResourceResponse<Document>(null)).Result);

            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(_json, _GoalPatch, _GoalId);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_UpdateAsync_ReturnsResourceWhenUpdated()
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

            _documentDbProvider.UpdateGoalsAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _GoalHttpTriggerService.UpdateAsync(_json, _GoalPatch, _GoalId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Goal>(result);

        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_GetGoalForCustomerAsync_ReturnsNullWhenResourceHasNotBeenFound()
        {
            _documentDbProvider.GetGoalForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).ReturnsNull();

            // Act
            var result = await _GoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PatchGoalHttpTriggerServiceTests_GetGoalForCustomerAsync_ReturnsResourceWhenResourceHasBeenFound()
        {
            _documentDbProvider.GetGoalForCustomerToUpdateAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_json).Result);

            // Act
            var result = await _GoalHttpTriggerService.GetGoalForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>());

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<string>(result);
        }
    }
}