using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using ElkLoggingLabApp.Data;
using ElkLoggingLabApp.Middleware;
using ElkLoggingLabApp.Services;

// Configure Serilog early for startup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "ElkLoggingLabApp")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
        new Uri(Environment.GetEnvironmentVariable("ELASTICSEARCH_URL") ?? "http://localhost:9200"))
    {
        AutoRegisterTemplate = true,
        AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
        IndexFormat = "elklab-logs-{0:yyyy.MM.dd}",
        NumberOfShards = 1,
        NumberOfReplicas = 0,
        MinimumLogEventLevel = LogEventLevel.Debug,
        EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback,
        FailureCallback = (e, ex) => Console.Error.WriteLine($"Elasticsearch sink error: {e.MessageTemplate} | {ex?.Message}"),
    })
    .CreateLogger();

try
{
    Log.Information("Starting ElkLoggingLabApp...");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog
    builder.Host.UseSerilog();

    // EF Core with SQLite
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=elklab.db");

        // Enable sensitive data logging in development & log query execution times
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        }

        options.LogTo(
            (eventId, level) => eventId.Id == Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.CommandExecuted.Id,
            eventData =>
            {
                if (eventData is Microsoft.EntityFrameworkCore.Diagnostics.CommandExecutedEventData cmdData)
                {
                    var duration = cmdData.Duration.TotalMilliseconds;
                    if (duration > 500)
                        Log.Error("EF SLOW QUERY ({Duration}ms): {Command}", duration, cmdData.Command.CommandText);
                    else if (duration > 100)
                        Log.Warning("EF MODERATE QUERY ({Duration}ms): {Command}", duration, cmdData.Command.CommandText);
                }
            });
    });

    // Register services
    builder.Services.AddScoped<ProductService>();
    builder.Services.AddScoped<OrderService>();
    builder.Services.AddScoped<AuditService>();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    var app = builder.Build();

    // Create and seed the database
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        Log.Information("Database initialized successfully");
    }

    // Middleware pipeline - order matters
    app.UseMiddleware<RequestLoggingMiddleware>();
    app.UseMiddleware<SecurityAuditMiddleware>();

    // Global exception handler
    app.UseExceptionHandler(errorApp =>
    {
        errorApp.Run(async context =>
        {
            var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
            if (exceptionFeature != null)
            {
                var correlationId = context.Items["CorrelationId"]?.ToString() ?? "N/A";
                Log.Error(exceptionFeature.Error,
                    "Unhandled exception | Path: {Path} | CorrelationId: {CorrelationId}",
                    context.Request.Path, correlationId);

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Internal Server Error",
                    correlationId,
                    message = app.Environment.IsDevelopment()
                        ? exceptionFeature.Error.Message
                        : "An unexpected error occurred"
                });
            }
        });
    });

    app.UseRouting();
    app.MapControllers();

    // Minimal API health endpoint
    app.MapGet("/", () => Results.Ok(new
    {
        application = "ELK Logging Lab",
        version = "1.0.0",
        documentation = "/api/diagnostics/health",
        endpoints = new[]
        {
            "GET  /api/products",
            "GET  /api/products/{id}",
            "GET  /api/products/search?name=&category=&minPrice=&maxPrice=",
            "GET  /api/products/slow",
            "POST /api/products",
            "PUT  /api/products/{id}",
            "DELETE /api/products/{id}",
            "GET  /api/orders",
            "GET  /api/orders/{id}",
            "POST /api/orders",
            "PATCH /api/orders/{id}/status",
            "DELETE /api/orders/{id}",
            "GET  /api/audit",
            "GET  /api/audit/search",
            "GET  /api/diagnostics/health",
            "GET  /api/diagnostics/slow-query",
            "GET  /api/diagnostics/exception",
            "GET  /api/diagnostics/memory-pressure",
            "GET  /api/diagnostics/log-burst"
        }
    }));

    Log.Information("ElkLoggingLabApp started. Listening on configured URLs.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
