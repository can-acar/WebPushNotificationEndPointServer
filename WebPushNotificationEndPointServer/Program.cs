using System.Reflection;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using EndpointEntities;
using Extensions;
using FluentValidation;
using HealthChecks.UI.Client;
using Lib;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using NewRelic.LogEnrichers.Serilog;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Ui.MsSqlServerProvider;
using Serilog.Ui.Web;
using WebPushNotificationEndPointServer;
using WebPushNotificationEndPointServer.Services;

try
{
    Log.Information("Starting web host");
    var assemblies = Assembly.GetExecutingAssembly();
    var builder = WebApplication.CreateBuilder(args);
    var host = builder.Host;
    var services = builder.Services;


    host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureLogging(x => x.ClearProviders().AddSerilog())
        .UseSerilog((ctx, lc) =>
        {
            if (builder.Environment.IsDevelopment())
            {
                lc.MinimumLevel.Debug();
            }
            else
            {
                lc.MinimumLevel.Information();
            }

            lc.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Error)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.DataProtection", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Internal.WebHost", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Server.WebListener", LogEventLevel.Information)
                .Enrich.WithThreadName()
                .Enrich.WithThreadId()
                .Enrich.WithExceptionDetails()
                .Enrich.WithNewRelicLogsInContext()
                .Enrich.WithProperty("ApplicationName", "Site.API")
                .Enrich.FromLogContext()
                .WriteTo.File(@"logs/log-.txt", fileSizeLimitBytes: 3000,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
                .ReadFrom.Configuration(ctx.Configuration);
        });

    host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
    {
        containerBuilder.RegisterModule(new ApiContainer());

        if (builder.Environment.IsDevelopment())
            containerBuilder.RegisterDbContext<PushSubscriptionContext>("Db");
        else
            containerBuilder.RegisterDbContext<PushSubscriptionContext>("Db");
    });


    services.AddCors();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo {Title = "Web Push Notification Endpoint Server", Version = "v1"}); });
    services.AddOptions();
    services.AddHealthChecks();
    // services.AddSingleton<VapidAuthorization>();
    // services.AddSingleton<PushNotificationService>();
    // services.AddSingleton<HttpClient>();

    //services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
    // services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    services.AddValidatorsFromAssembly(assemblies);
    services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(assemblies); });
    services.AddSerilogUi(options => options.UseSqlServer("DB", "logs"));

    // services.Configure<IISOptions>(options => { options.ForwardClientCertificate = false; });
    services.Configure<FormOptions>(o =>
    {
        o.ValueLengthLimit = int.MaxValue;
        o.MultipartBodyLengthLimit = long.MaxValue; // <-- !!! long.MaxValue
        o.MultipartBoundaryLengthLimit = int.MaxValue;
        o.MultipartHeadersCountLimit = int.MaxValue;
        o.MultipartHeadersLengthLimit = int.MaxValue;
    });

    // services.AddDataProtection();

    services.AddControllers()
        .ConfigureApiBehaviorOptions(options =>
        {
            options.SuppressConsumesConstraintForFormFileParameters = true;
            options.SuppressInferBindingSourcesForParameters = true;
            options.SuppressModelStateInvalidFilter = true;
            options.SuppressMapClientErrors = true;
            options.ClientErrorMapping[404]
                .Link = "https://httpstatuses.com/404";
        })
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.Formatting = Formatting.None;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
            options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;
            options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            options.SerializerSettings.DateFormatString = "dd.MM.yyyy HH:mm:ss";
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            options.SerializerSettings.FloatFormatHandling = FloatFormatHandling.String;
            options.SerializerSettings.FloatParseHandling = FloatParseHandling.Double;
        });


    builder.WebHost.UseKestrel();
    builder.WebHost.UseIISIntegration();
    builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());

    var app = builder.Build();

    ((IApplicationBuilder) app).ApplicationServices.GetAutofacRoot();

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    //app.UseMiddleware<ErrorHandlerMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Push Notification Endpoint Server v1"));
    }

    app.UseHealthChecks("/health", new HealthCheckOptions {Predicate = _ => true});

    app.UseHealthChecks("/healthz",
        new HealthCheckOptions {Predicate = _ => true, ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse});


    app.UseRouting();

    app.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true));

    app.MapControllers();

    app.UseSerilogUi();

    app.MapHealthChecks("/health/ready", new HealthCheckOptions() {Predicate = (check) => check.Tags.Contains("ready")});

    app.MapHealthChecks("/health/live", new HealthCheckOptions()
    {
        // Exclude all checks and return a 200-Ok.
        Predicate = (_) => false
    });


    app.Run();


// // Add services to the container.
//
//     builder.Services.AddControllers();
// // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//     builder.Services.AddEndpointsApiExplorer();
//     builder.Services.AddSwaggerGen();
//
//     var app = builder.Build();
//
// // Configure the HTTP request pipeline.
//     if (app.Environment.IsDevelopment())
//     {
//         app.UseSwagger();
//         app.UseSwaggerUI();
//     }
//
//     app.UseHttpsRedirection();
//
//     app.UseAuthorization();
//
//     app.MapControllers();
//
//     app.Run();
}
catch (Exception ex)
{
    var type = ex.GetType().Name;
    if (type.Equals("StopTheHostException", StringComparison.Ordinal))
    {
        throw;
    }

    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.CloseAndFlush();
}