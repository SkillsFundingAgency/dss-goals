using System.Net;
using System.Net.Http;
using System.Reflection;
using DFC.Functions.DI.Standard.Attributes;
using DFC.Swagger.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace NCS.DSS.Goal.APIDefinition
{
    public class ApiDefinition
    {
        public const string APITitle = "Goals";
        public const string APIDefinitionName = "API-Definition";
        public const string APIDefRoute = APITitle + "/" + APIDefinitionName;
        public const string APIDescription = "To support the Data Collections integration with DSS SubcontractorId has been added as an attribute.";
        public const string ApiVersion = "2.0.0";
        private ISwaggerDocumentGenerator swaggerDocumentGenerator;

        public ApiDefinition(ISwaggerDocumentGenerator swaggerDocumentGenerator)
        {
            this.swaggerDocumentGenerator = swaggerDocumentGenerator;
        }

        [FunctionName(APIDefinitionName)]
        public HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = APIDefRoute)] HttpRequest req)
        {
            var swagger = swaggerDocumentGenerator.GenerateSwaggerDocument(req, APITitle, APIDescription,
                APIDefinitionName, ApiVersion, Assembly.GetExecutingAssembly());

            if (string.IsNullOrEmpty(swagger))
                return new HttpResponseMessage(HttpStatusCode.NoContent);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(swagger)
            };
        }
    }
}