using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.PostGoalsHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServiceTests
{
    [TestFixture]
    public class PostGoalHttpTriggerServiceTests
    {
        private IPostGoalsHttpTriggerService _GoalHttpTriggerService;
        private IDocumentDBProvider _documentDbProvider;
        private string _json;
        private Models.Goal _Goal;
        private readonly Guid _GoalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = Substitute.For<IDocumentDBProvider>();
            _GoalHttpTriggerService = Substitute.For<PostGoalsHttpTriggerService>(_documentDbProvider);
            _Goal = Substitute.For<Models.Goal>();
            _json = JsonConvert.SerializeObject(_Goal);
        }

        [Test]
        public async Task PostGoalHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenGoalJsonIsNull()
        {
            // Act
            var result = await _GoalHttpTriggerService.CreateAsync(Arg.Any<Models.Goal>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public async Task PostGoalHttpTriggerServiceTests_CreateAsync_ReturnsResource()
        {
            const string documentServiceResponseClass = "Microsoft.Azure.Documents.DocumentServiceResponse, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";
            const string dictionaryNameValueCollectionClass = "Microsoft.Azure.Documents.Collections.DictionaryNameValueCollection, Microsoft.Azure.DocumentDB.Core, Version=2.2.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35";

            var resourceResponse = new ResourceResponse<Document>(new Document());
            var documentServiceResponseType = Type.GetType(documentServiceResponseClass);

            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;

            var headers = new NameValueCollection { { "x-ms-request-charge", "0" } };

            var headersDictionaryType = Type.GetType(dictionaryNameValueCollectionClass);

            var headersDictionaryInstance = Activator.CreateInstance(headersDictionaryType, headers);

            var arguments = new[] { Stream.Null, headersDictionaryInstance, HttpStatusCode.Created, null };

            var documentServiceResponse = documentServiceResponseType.GetTypeInfo().GetConstructors(flags)[0].Invoke(arguments);

            var responseField = typeof(ResourceResponse<Document>).GetTypeInfo().GetField("response", flags);

            responseField?.SetValue(resourceResponse, documentServiceResponse);

            _documentDbProvider.CreateGoalsAsync(Arg.Any<Models.Goal>()).Returns(Task.FromResult(resourceResponse).Result);

            // Act
            var result = await _GoalHttpTriggerService.CreateAsync(_Goal);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOf<Models.Goal>(result);

        }
    }
}