using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Order.Api.Features.AddOrderItem;
using Order.Api.Features.CancelOrder;
using Order.Api.Features.CreateOrder;
using Order.Api.Features.DeleteOrder;
using Order.Api.Features.GetAllOrders;
using Order.Api.Features.GetOrder;
using Order.Api.Features.GetOrdersByCustomer;
using Order.Api.Features.GetOrdersByCustomerAndStatus;
using Order.Api.Features.UpdateOrderStatus;
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
        "CreateOrder"         => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new CreateOrderFunction().Handler, serializer).Build().RunAsync(),
        "GetOrder"            => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new GetOrderFunction().Handler, serializer).Build().RunAsync(),
        "GetAllOrders"        => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new GetAllOrdersFunction().Handler, serializer).Build().RunAsync(),
        "GetOrdersByCustomer" => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new GetOrdersByCustomerFunction().Handler, serializer).Build().RunAsync(),
        "AddOrderItem"        => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new AddOrderItemFunction().Handler, serializer).Build().RunAsync(),
        "UpdateOrderStatus"   => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new UpdateOrderStatusFunction().Handler, serializer).Build().RunAsync(),
        "CancelOrder"         => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new CancelOrderFunction().Handler, serializer).Build().RunAsync(),
        "DeleteOrder"                    => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new DeleteOrderFunction().Handler, serializer).Build().RunAsync(),
        "GetOrdersByCustomerAndStatus"   => LambdaBootstrapBuilder.Create<APIGatewayProxyRequest, APIGatewayProxyResponse>(new GetOrdersByCustomerAndStatusFunction().Handler, serializer).Build().RunAsync(),
        _                                => throw new InvalidOperationException($"Unknown handler: {handler}")
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
