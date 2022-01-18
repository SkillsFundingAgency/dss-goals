using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using DFC.Common.Standard.Logging;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NCS.DSS.Goal;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;


[assembly: FunctionsStartup(typeof(Startup))]

namespace NCS.DSS.Goal
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

            builder.Services.AddScoped<IGoalPatchService, GoalPatchService>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddScoped<IGetGoalHttpTriggerService, GetGoalHttpTriggerService>();
            builder.Services.AddScoped<IGetGoalByIdHttpTriggerService, GetGoalByIdHttpTriggerService>();
            builder.Services.AddScoped<IPostGoalHttpTriggerService, PostGoalHttpTriggerService>();
            builder.Services.AddScoped<IPatchGoalHttpTriggerService, PatchGoalHttpTriggerService>();
        }
    }
}