using Serilog;
using Serilog.Templates;
using Serilog.Templates.Themes;

namespace AnalysisService.Utils;

public static class SerilogConfiguration
{
    public static void ConfigureLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console(new ExpressionTemplate(
                "[{@t:HH:mm:ss} {@l:u3} ({SourceContext})] {@m}\n{@x}", theme: TemplateTheme.Code))
            .Enrich.FromLogContext()
            .CreateLogger();
    }
}