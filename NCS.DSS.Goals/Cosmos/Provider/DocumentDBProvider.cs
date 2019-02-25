using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Goals.Cosmos.Client;
using NCS.DSS.Goals.Cosmos.Helper;
using NCS.DSS.Goals.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Goals.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }


        public bool DoesInteractionResourceExistAndBelongToCustomer(Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateInteractionDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM interactions i " +
                                "WHERE i.id = @interactionId " +
                                "AND i.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public bool DoesActionPlanResourceExistAndBelongToCustomer(Guid actionPlanId, Guid interactionId, Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateActionPlanDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var query = client.CreateDocumentQuery<long>(collectionUri, new SqlQuerySpec()
                {
                    QueryText = "SELECT VALUE COUNT(1) FROM actionplans a " +
                                "WHERE a.id = @actionPlanId " +
                                "AND a.InteractionId = @interactionId " +
                                "AND a.CustomerId = @customerId",

                    Parameters = new SqlParameterCollection()
                    {
                        new SqlParameter("@actionPlanId", actionPlanId),
                        new SqlParameter("@interactionId", interactionId),
                        new SqlParameter("@customerId", customerId)
                    }
                }).AsEnumerable().FirstOrDefault();

                return Convert.ToBoolean(Convert.ToInt16(query));
            }
            catch (DocumentQueryException)
            {
                return false;
            }

        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }


        public async Task<string> GetGoalForCustomerToUpdateAsync(Guid customerId, Guid goalId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var goalForCustomerQuery = client
                ?.CreateDocumentQuery<Goal>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.GoalId == goalId)
                .AsDocumentQuery();

            if (goalForCustomerQuery == null)
                return null;

            var goals = await goalForCustomerQuery.ExecuteNextAsync();

            return goals?.FirstOrDefault()?.ToString();
        }


        public async Task<List<Goal>> GetAllGoalsForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var GoalsQuery = client.CreateDocumentQuery<Goal>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var Goals = new List<Goal>();

            while (GoalsQuery.HasMoreResults)
            {
                var response = await GoalsQuery.ExecuteNextAsync<Goal>();
                Goals.AddRange(response);
            }

            return Goals.Any() ? Goals : null;
        }

        public async Task<Goal> GetGoalsForCustomerAsync(Guid customerId, Guid goalId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var goalForCustomerQuery = client
                ?.CreateDocumentQuery<Goal>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.GoalId == goalId)
                .AsDocumentQuery();

            if (goalForCustomerQuery == null)
                return null;

            var goals = await goalForCustomerQuery.ExecuteNextAsync<Goal>();

            return goals?.FirstOrDefault();
        }

        public async Task<ResourceResponse<Document>> CreateGoalsAsync(Goal Goals)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, Goals);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateGoalAsync(string goalJson, Guid goalId)
        {

            if (string.IsNullOrEmpty(goalJson))
                return null;

            var documentUri = DocumentDBHelper.CreateDocumentUri(goalId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var goalDocumentJObject = JObject.Parse(goalJson);

            var response = await client.ReplaceDocumentAsync(documentUri, goalDocumentJObject);

            return response;
        }

        public async Task<bool> DeleteAsync(Guid goalId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(goalId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            var response = await client.DeleteDocumentAsync(documentUri);

            return response.StatusCode == HttpStatusCode.OK;
        }




    }
}