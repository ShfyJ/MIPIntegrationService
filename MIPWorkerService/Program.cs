using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using MIPWorkerService.Workers;
using MIPWorkerService.Configuration;
using FluentMigrator.Runner;
using MIPSharedLibrary.Migrations;
using MIPWorkerService.Services;

var builder = Host.CreateDefaultBuilder(args);

// Build the configuration explicitly
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.UseSerilog();

// Configure Services
builder.ConfigureServices((context, services) =>
{
    var configuration = context.Configuration;

    // Get connection strings from appsettings.json
    var sourceDbConnectionString = configuration["SourceDatabase:ConnectionString"];
    var targetDbConnectionString = configuration["TargetDatabase:ConnectionString"];

    // Register DatabaseConfig for accessing both connection strings
    services.AddSingleton(new DatabaseConfig(sourceDbConnectionString, targetDbConnectionString));

    // Add FluentMigrator to the DI container
    services.AddFluentMigratorCore()
        .ConfigureRunner(runner => runner
            .AddSqlServer()
            .WithGlobalConnectionString(targetDbConnectionString)
            .ScanIn(typeof(CreateSentDataTable).Assembly).For.Migrations())
        .AddLogging(lb => lb.AddFluentMigratorConsole());

    // Load Private Key from Environment Variables
    var privateKeyPart1 = Environment.GetEnvironmentVariable("PRIVATE_KEY_MIP_API_PART1");
    var privateKeyPart2 = Environment.GetEnvironmentVariable("PRIVATE_KEY_MIP_API_PART2");
    var privateKey = $"{privateKeyPart1}{privateKeyPart2}";
    privateKey = privateKey.Replace("\\n", "\n");

    // Register JwtTokenService with private key and configuration
    services.AddSingleton(new JwtTokenManager(privateKey, configuration));

    // Add the Worker class as a Hosted Service
    services.AddHostedService<Worker>();

    services.AddSingleton<IDataService, DataService>();
    services.AddHttpClient<IHttpService, HttpService>();
});

try
{
    Log.Information("Starting MIPWorkerService...");
    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        // Execute the migrations
        runner.MigrateUp();
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "MIPWorkerService terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
