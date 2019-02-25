using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Goals.Cosmos.Provider;

namespace NCS.DSS.Goals.GetGoalsHttpTrigger.Service
{
    public class GetGoalsHttpTriggerService : IGetGoalsHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetGoalsHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Goal>> GetGoalsAsync(Guid customerId)
        {
            var Goals = await _documentDbProvider.GetAllGoalsForCustomerAsync(customerId);

            return Goals;
        }
    }
}