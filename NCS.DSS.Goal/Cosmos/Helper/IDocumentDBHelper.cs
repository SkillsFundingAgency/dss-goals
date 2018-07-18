using System;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid goalPlanId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateIntergoalDocumentCollectionUri();
    }
}