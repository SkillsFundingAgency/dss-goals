using System;

namespace NCS.DSS.Goal.Cosmos.Helper
{
    public interface IDocumentDBHelper
    {
        Uri CreateDocumentCollectionUri();
        Uri CreateDocumentUri(Guid actionPlanId);
        Uri CreateCustomerDocumentCollectionUri();
        Uri CreateInteractionDocumentCollectionUri();
    }
}