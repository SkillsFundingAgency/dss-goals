﻿using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public static class DocumentDBHelper
    {
        private static Uri _documentCollectionUri;
        private static readonly string DatabaseId = Environment.GetEnvironmentVariable("DatabaseId");
        private static readonly string CollectionId = Environment.GetEnvironmentVariable("CollectionId");

        private static Uri _customerDocumentCollectionUri;
        private static readonly string CustomerDatabaseId = Environment.GetEnvironmentVariable("CustomerDatabaseId");
        private static readonly string CustomerCollectionId = Environment.GetEnvironmentVariable("CustomerCollectionId");

        private static Uri _interactionDocumentCollectionUri;
        private static readonly string InteractionDatabaseId = Environment.GetEnvironmentVariable("InteractionDatabaseId");
        private static readonly string InteractionCollectionId = Environment.GetEnvironmentVariable("InteractionCollectionId");

        private static Uri _actionPlanDocumentCollectionUri;
        private static readonly string ActionPlanDatabaseId = Environment.GetEnvironmentVariable("ActionPlanDatabaseId");
        private static readonly string ActionPlanCollectionId = Environment.GetEnvironmentVariable("ActionPlanCollectionId");

        public static Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                DatabaseId,
                CollectionId);

            return _documentCollectionUri;
        }

        public static Uri CreateDocumentUri(Guid actionPlanId)
        {
            return UriFactory.CreateDocumentUri(DatabaseId, CollectionId, actionPlanId.ToString());
        }

        #region CustomerDB

        public static Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                CustomerDatabaseId, CustomerCollectionId);

            return _customerDocumentCollectionUri;
        }

        public static Uri CreateCustomerDocumentUri(Guid customerId)
        {
            return UriFactory.CreateDocumentUri(CustomerDatabaseId, CustomerCollectionId, customerId.ToString());
        }

        #endregion

        #region InteractionDB

        public static Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                InteractionDatabaseId, InteractionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        public static Uri CreateInteractionDocumentUri(Guid interactionId)
        {
            return UriFactory.CreateDocumentUri(InteractionDatabaseId, InteractionCollectionId, interactionId.ToString()); ;
        }

        #endregion

        #region ActionPlanDB

        public static Uri CreateActionPlanDocumentCollectionUri()
        {
            if (_actionPlanDocumentCollectionUri != null)
                return _actionPlanDocumentCollectionUri;

            _actionPlanDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                ActionPlanDatabaseId, ActionPlanCollectionId);

            return _actionPlanDocumentCollectionUri;
        }

        public static Uri CreateActionPlanDocumentUri(Guid actionPlanId)
        {
            return UriFactory.CreateDocumentUri(ActionPlanDatabaseId, ActionPlanCollectionId, actionPlanId.ToString());
        }

        #endregion   

    }
}
