using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;
using Server.Services;

namespace Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureLogging(this IServiceCollection services)
    {
        services.AddSerilog(loggerConfiguration =>
        {
            loggerConfiguration.MinimumLevel.Information();

            loggerConfiguration.WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3} ({SourceContext})] {@m}\n{@x}",
                theme: TemplateTheme.Code
            ));
        });
    }

    public static void AddServices(this IServiceCollection services)
    {
        services.AddHttpClient<IHealthCheckService, HealthCheckService>();
    }
}