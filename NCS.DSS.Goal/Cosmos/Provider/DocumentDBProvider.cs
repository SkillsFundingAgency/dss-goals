using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Goal.Cosmos.Client;
using NCS.DSS.Goal.Cosmos.Helper;

namespace NCS.DSS.Goal.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var customerQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return customerQuery.Where(x => x.Id == customerId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateCustomerDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var customerByIdQuery = client
                ?.CreateDocumentQuery<Document>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.Id == customerId.ToString())
                .AsDocumentQuery();

            if (customerByIdQuery == null)
                return false;

            var customerQuery = await customerByIdQuery.ExecuteNextAsync<Document>();

            var customer = customerQuery?.FirstOrDefault();

            if (customer == null)
                return false;

            var dateOfTermination = customer.GetPropertyValue<DateTime?>("DateOfTermination");

            return dateOfTermination.HasValue;
        }

        public bool DoesInteractionResourceExist(Guid interactionId)
        {
            var collectionUri = _documentDbHelper.CreateInteractionDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var intergoalQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return intergoalQuery.Where(x => x.Id == interactionId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public bool DoesActionPlanResourceExist(Guid actionPlanId)
        {
            var collectionUri = _documentDbHelper.CreateActionPlanDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var goalPlanQuery = client.CreateDocumentQuery<Document>(collectionUri, new FeedOptions() { MaxItemCount = 1 });
            return goalPlanQuery.Where(x => x.Id == actionPlanId.ToString()).Select(x => x.Id).AsEnumerable().Any();
        }

        public async Task<List<Models.Goal>> GetGoalsForCustomerAsync(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var goalsQuery = client.CreateDocumentQuery<Models.Goal>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var goals = new List<Models.Goal>();

            while (goalsQuery.HasMoreResults)
            {
                var response = await goalsQuery.ExecuteNextAsync<Models.Goal>();
                goals.AddRange(response);
            }

            return goals.Any() ? goals : null;
        }

        public async Task<Models.Goal> GetGoalForCustomerAsync(Guid customerId, Guid goalId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            var goalForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Goal>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.GoalId == goalId)
                .AsDocumentQuery();

            if (goalForCustomerQuery == null)
                return null;

            var goals = await goalForCustomerQuery.ExecuteNextAsync<Models.Goal>();

            return goals?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateGoalAsync(Models.Goal goal)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, goal);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateGoalAsync(Models.Goal goal)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(goal.GoalId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, goal);

            return response;
        }
    }
}