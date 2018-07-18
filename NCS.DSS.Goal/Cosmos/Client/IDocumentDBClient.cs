using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Goal.Cosmos.Client
{
    public interface IDocumentDBClient
    {
        DocumentClient CreateDocumentClient();
    }
}