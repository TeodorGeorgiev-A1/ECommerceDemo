namespace BillingService;

public class Program
{
    public static void Main(string[] args)
    {
        // RUN COMMAND: "dapr run --app-id billingservice --app-port 12102 --dapr-http-port 3502 --dapr-grpc-port 50002 --components-path ./components"
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            })
            .AddDapr();
        builder.Services.AddDaprClient(builder =>
        {
            builder.UseGrpcEndpoint("http://localhost:50002");
        });
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
