using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;

namespace NCS.DSS.Goal
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices(services =>
                {
                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<IDocumentDBProvider, DocumentDBProvider>();

                    services.AddScoped<IGoalPatchService, GoalPatchService>();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddScoped<IGetGoalHttpTriggerService, GetGoalHttpTriggerService>();
                    services.AddScoped<IGetGoalByIdHttpTriggerService, GetGoalByIdHttpTriggerService>();
                    services.AddScoped<IPostGoalHttpTriggerService, PostGoalHttpTriggerService>();
                    services.AddScoped<IPatchGoalHttpTriggerService, PatchGoalHttpTriggerService>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();

                    services.Configure<LoggerFilterOptions>(options =>
                    {
                        LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
                            == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
                        if (toRemove is not null)
                        {
                            options.Rules.Remove(toRemove);
                        }
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}
