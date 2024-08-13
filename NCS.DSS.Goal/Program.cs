using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NCS.DSS.Goal.Cosmos.Helper;
using NCS.DSS.Goal.Cosmos.Provider;
using NCS.DSS.Goal.GetGoalByIdHttpTrigger.Service;
using NCS.DSS.Goal.GetGoalHttpTrigger.Service;
using NCS.DSS.Goal.Helpers;
using NCS.DSS.Goal.PatchGoalHttpTrigger.Service;
using NCS.DSS.Goal.PostGoalHttpTrigger.Service;
using NCS.DSS.Goal.Validation;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddSingleton<IResourceHelper, ResourceHelper>();
        services.AddSingleton<IValidate, Validate>();
        services.AddSingleton<ILoggerHelper, LoggerHelper>();
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
    })
    .Build();

host.Run();
