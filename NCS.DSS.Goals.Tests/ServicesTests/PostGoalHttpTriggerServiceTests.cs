using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using Newtonsoft.Json;
using Moq;
using NUnit.Framework;

namespace NCS.DSS.Goal.Tests.ServicesTests
{
    [TestFixture]
    public class PostGoalHttpTriggerServiceTests
    {
        private IPostGoalHttpTriggerService _goalHttpTriggerService;
        private Mock<IDocumentDBProvider> _documentDbProvider;
        private string _json;
        private Models.Goal _goal;
        private readonly Guid _goalId = Guid.Parse("7E467BDB-213F-407A-B86A-1954053D3C24");

        [SetUp]
        public void Setup()
        {
            _documentDbProvider = new Mock<IDocumentDBProvider>();
            _goalHttpTriggerService = new PostGoalHttpTriggerService(_documentDbProvider.Object);
            _goal = new Models.Goal();
            _json = JsonConvert.SerializeObject(_goal);
        }

        [Test]
        public async Task PostActionPlanHttpTriggerServiceTests_CreateAsync_ReturnsNullWhenActionPlanJsonIsNull()
        {
            // Act
            var result = await _goalHttpTriggerService.CreateAsync(It.IsAny<Models.Goal>());

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task PostActionPlanHttpTriggerServiceTests_CreateAsync_ReturnsResource()
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

            _documentDbProvider.Setup(x => x.CreateGoalAsync(_goal)).Returns(Task.FromResult(resourceResponse));

            // Act
            var result = await _goalHttpTriggerService.CreateAsync(_goal);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Models.Goal>());

        }
    }
}