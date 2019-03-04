using System;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;


namespace NCS.DSS.Goal.Ioc
{
    public class RegisterServiceProvider
    {
        public IServiceProvider CreateServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddTransient<IGetGoalHttpTriggerService, GetGoalHttpTriggerService>();
            services.AddTransient<IGetGoalByIdHttpTriggerService, GetGoalByIdHttpTriggerService>();
            services.AddTransient<IPostGoalHttpTriggerService, PostGoalHttpTriggerService>();
            services.AddTransient<IPatchGoalHttpTriggerService, PatchGoalHttpTriggerService>();
            services.AddTransient<IGoalsPatchService, GoalsPatchService>();

            services.AddTransient<IResourceHelper, ResourceHelper>();
            services.AddTransient<IValidate, Validate>();
            services.AddTransient<IHttpRequestMessageHelper, HttpRequestMessageHelper>();
            services.AddTransient<IDocumentDBProvider, DocumentDBProvider>();

            return services.BuildServiceProvider(true);
        }
    }
}
