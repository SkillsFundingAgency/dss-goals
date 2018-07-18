﻿
using System;
using System.Configuration;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public class DocumentDBHelper : IDocumentDBHelper
    {
        private Uri _documentCollectionUri;
        private Uri _documentUri;
        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];

        private Uri _customerDocumentCollectionUri;
        private readonly string _customerDatabaseId = ConfigurationManager.AppSettings["CustomerDatabaseId"];
        private readonly string _customerCollectionId = ConfigurationManager.AppSettings["CustomerCollectionId"];

        private Uri _interactionDocumentCollectionUri;
        private readonly string _interactionDatabaseId = ConfigurationManager.AppSettings["InteractionDatabaseId"];
        private readonly string _interactionCollectionId = ConfigurationManager.AppSettings["InteractionCollectionId"];

        private Uri _actionPlanDocumentCollectionUri;
        private readonly string _actionPlanDatabaseId = ConfigurationManager.AppSettings["ActionPlanDatabaseId"];
        private readonly string _actionPlanCollectionId = ConfigurationManager.AppSettings["ActionPlanCollectionId"];

        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid actionPlanId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, actionPlanId.ToString());

            return _documentUri;

        }

        #region CustomerDB

        public Uri CreateCustomerDocumentCollectionUri()
        {
            if (_customerDocumentCollectionUri != null)
                return _customerDocumentCollectionUri;

            _customerDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _customerDatabaseId, _customerCollectionId);

            return _customerDocumentCollectionUri;
        }

        #endregion

        #region InteractionDB

        public Uri CreateInteractionDocumentCollectionUri()
        {
            if (_interactionDocumentCollectionUri != null)
                return _interactionDocumentCollectionUri;

            _interactionDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _interactionDatabaseId, _interactionCollectionId);

            return _interactionDocumentCollectionUri;
        }

        #endregion

        #region ActionPlanDB

        public Uri CreateActionPlanDocumentCollectionUri()
        {
            if (_actionPlanDocumentCollectionUri != null)
                return _actionPlanDocumentCollectionUri;

            _actionPlanDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _actionPlanDatabaseId, _actionPlanCollectionId);

            return _actionPlanDocumentCollectionUri;
        }

        #endregion   

    }
}
