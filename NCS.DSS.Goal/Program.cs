using Azure.Identity;
using Azure.Messaging.ServiceBus;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.Models;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.ServiceBus;
using NCS.DSS.Goal.Validation;

namespace NCS.DSS.Goal
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {

            var host = new HostBuilder()
                .ConfigureFunctionsWebApplication()
                .ConfigureServices((context, services) =>
                {
                    var configuration = context.Configuration;
                    services.AddOptions<GoalConfigurationSettings>()
                        .Bind(configuration);

                    services.AddApplicationInsightsTelemetryWorkerService();
                    services.ConfigureFunctionsApplicationInsights();
                    services.AddSingleton<IResourceHelper, ResourceHelper>();
                    services.AddSingleton<IValidate, Validate>();
                    services.AddSingleton<IHttpRequestHelper, HttpRequestHelper>();
                    services.AddSingleton<IHttpResponseMessageHelper, HttpResponseMessageHelper>();
                    services.AddSingleton<IJsonHelper, JsonHelper>();
                    services.AddSingleton<ICosmosDbProvider, CosmosDbProvider>();
                    services.AddSingleton<IGoalServiceBusClient, GoalServiceBusClient>();
                    services.AddScoped<IGoalPatchService, GoalPatchService>();
                    services.AddScoped<ISwaggerDocumentGenerator, SwaggerDocumentGenerator>();
                    services.AddScoped<IGetGoalHttpTriggerService, GetGoalHttpTriggerService>();
                    services.AddScoped<IGetGoalByIdHttpTriggerService, GetGoalByIdHttpTriggerService>();
                    services.AddScoped<IPostGoalHttpTriggerService, PostGoalHttpTriggerService>();
                    services.AddScoped<IPatchGoalHttpTriggerService, PatchGoalHttpTriggerService>();
                    services.AddSingleton<IDynamicHelper, DynamicHelper>();

                    services.AddSingleton(s =>
                    {
                        var logger = s.GetRequiredService<ILogger<Program>>();

                        var connectionString = configuration["GoalConnectionString"];
                        var endpoint = configuration["CosmosDbEndpoint"];

                        var options = new CosmosClientOptions
                        {
                            ConnectionMode = ConnectionMode.Gateway
                        };

                        if (!string.IsNullOrWhiteSpace(endpoint))
                        {
                            logger.LogInformation("Using DefaultAzureCredential for Cosmos DB (managed identity)");
                            return new CosmosClient(endpoint, new DefaultAzureCredential(), options);
                        }
                        else if (!string.IsNullOrWhiteSpace(connectionString))
                        {
                            logger.LogInformation("No managed identity found: using Cosmos DB connection string (local development)");
                            return new CosmosClient(connectionString, options);
                        }
                        else
                        {
                            throw new InvalidOperationException("Neither CosmosDbEndpoint or a ConnectionString are configured");
                        }
                    });

                    services.AddSingleton(s =>
                    {
                        var settings = s.GetRequiredService<IOptions<GoalConfigurationSettings>>().Value;

                        return new ServiceBusClient(settings.ServiceBusConnectionString);
                    });

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
