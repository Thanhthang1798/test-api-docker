using DemoDockerAPI2.Services;
namespace DemoDockerAPI2.Options;

public static class DependencyInjection
{
    public static IServiceCollection AddDockerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DeliveryOptions>(configuration.GetSection("Delivery"));


        // Service
        services.AddScoped<IEmailService, EMailService>();
        return services;
    }
}