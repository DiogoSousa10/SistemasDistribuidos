using Data.Services;
using GrpcServer;
using GrpcServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddScoped<UserUtils>();
builder.Services.AddSingleton<RabbitConfiguration>(provider =>
{
    // Aqui voc� pode fornecer os valores necess�rios para as depend�ncias do tipo string
    string exchangeName = "NOME_DA_EXCHANGE";
    string routingKey = "NOME_ROUTING_KEY";

    // Retorne uma inst�ncia de RabbitConfiguration com as depend�ncias resolvidas
    return new RabbitConfiguration(exchangeName, routingKey);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ProvisioningService>();
app.MapGrpcService<UserService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");


app.Run();
