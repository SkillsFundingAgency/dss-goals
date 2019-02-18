using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalsByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalsHttpTrigger.Service;
using NCS.DSS.Goal.Ioc;
using NCS.DSS.Goal.PatchGoalsHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalsHttpTrigger.Service;
using NCS.DSS.Goal.Validation;

[assembly: WebJobsStartup(typeof(WebJobsExtensionStartup), "Web Jobs Extension Startup")]

namespace NCS.DSS.Goal.Ioc
{
    public class WebJobsExtensionStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddDependencyInjection();

            builder.Services.AddSingleton<IResourceHelper, ResourceHelper>();
            builder.Services.AddSingleton<IValidate, Validate>();
            builder.Services.AddSingleton<ILoggerHelper, LoggerHelper>();
            builder.Services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
            builder.Services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
            builder.Services.AddSingleton<IJsonHelper, JsonHelper>();
            builder.Services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

            builder.Services.AddScoped<IGoalsPatchService, GoalsPatchService>();
            builder.Services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
            builder.Services.AddScoped<IGetGoalsHttpTriggerService, GetGoalsHttpTriggerService>();
            builder.Services.AddScoped<IGetGoalsByIdHttpTriggerService, GetGoalsByIdHttpTriggerService>();
            builder.Services.AddScoped<IPostGoalsHttpTriggerService, PostGoalsHttpTriggerService>();
            builder.Services.AddScoped<IPatchGoalsHttpTriggerService, PatchGoalsHttpTriggerService>();
        }
    }
}