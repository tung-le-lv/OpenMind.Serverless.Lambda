using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.Configuration;
using Payment.Api.Features.ProcessPayment;
using Serilog;

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var handler = Environment.GetEnvironmentVariable("LAMBDA_HANDLER")
    ?? throw new InvalidOperationException("LAMBDA_HANDLER environment variable is required.");

var serializer = new DefaultLambdaJsonSerializer();

try
{
    await (handler switch
    {
        "ProcessPayment" => LambdaBootstrapBuilder.Create<SQSEvent, SQSBatchResponse>(new ProcessPaymentFunction().Handler, serializer).Build().RunAsync(),
        _ => throw new InvalidOperationException($"Unknown handler: {handler}")
    });
}
catch (Exception ex)
{
    Log.Fatal(ex, "Lambda bootstrap failed");
}
finally
{
    await Log.CloseAndFlushAsync();
}
