namespace BillingService;

public class Program
{
    public static void Main(string[] args)
    {
        // SIDECAR COMMAND: "dapr run --app-id billingservice --app-port 12102 --dapr-http-port 3502 --dapr-grpc-port 50002"
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddDapr();
        builder.Services.AddDaprClient();
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
