
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

        private Uri _intergoalDocumentCollectionUri;
        private readonly string _intergoalDatabaseId = ConfigurationManager.AppSettings["IntergoalDatabaseId"];
        private readonly string _intergoalCollectionId = ConfigurationManager.AppSettings["IntergoalCollectionId"];

        private Uri _goalPlanDocumentCollectionUri;
        private readonly string _goalPlanDatabaseId = ConfigurationManager.AppSettings["GoalPlanDatabaseId"];
        private readonly string _goalPlanCollectionId = ConfigurationManager.AppSettings["GoalPlanCollectionId"];

        public Uri CreateDocumentCollectionUri()
        {
            if (_documentCollectionUri != null)
                return _documentCollectionUri;

            _documentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _databaseId,
                _collectionId);

            return _documentCollectionUri;
        }
        
        public Uri CreateDocumentUri(Guid goalPlanId)
        {
            if (_documentUri != null)
                return _documentUri;

            _documentUri = UriFactory.CreateDocumentUri(_databaseId, _collectionId, goalPlanId.ToString());

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

        #region IntergoalDB

        public Uri CreateIntergoalDocumentCollectionUri()
        {
            if (_intergoalDocumentCollectionUri != null)
                return _intergoalDocumentCollectionUri;

            _intergoalDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _intergoalDatabaseId, _intergoalCollectionId);

            return _intergoalDocumentCollectionUri;
        }

        #endregion

        #region GoalPlanDB

        public Uri CreateGoalPlanDocumentCollectionUri()
        {
            if (_goalPlanDocumentCollectionUri != null)
                return _goalPlanDocumentCollectionUri;

            _goalPlanDocumentCollectionUri = UriFactory.CreateDocumentCollectionUri(
                _goalPlanDatabaseId, _goalPlanCollectionId);

            return _goalPlanDocumentCollectionUri;
        }

        #endregion   

    }
}
