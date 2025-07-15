namespace OrderService;

public class Program
{
    public static void Main(string[] args)
    {
        // RUN: "dapr run --app-id orderservice --app-port 12100 --dapr-http-port 3500 -- dotnet run"
        // TO START THIS SERVICE
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers().AddDapr(); // Add Dapr support

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();


        app.MapControllers();
        app.MapSubscribeHandler(); // Enables pub/sub handling

        app.Run();
    }
}
