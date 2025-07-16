namespace OrderService;

public class Program
{
    public static void Main(string[] args)
    {
        // RUN COMMAND: "dapr run --app-id orderservice --app-port 12100 --dapr-http-port 3500 --dapr-grpc-port 50000 --components-path ./components"

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddDaprClient(builder =>
        {
            builder.UseGrpcEndpoint("http://localhost:50000");
        });
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
                {
                    options.SuppressModelStateInvalidFilter = true;
                })
            .AddDapr(); // Add Dapr support
        builder.Services.AddHttpClient<InventoryClient>();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();


        app.UseRouting();
        app.UseCloudEvents();
        app.MapControllers();
        app.MapSubscribeHandler();

        app.Run();
    }
}
