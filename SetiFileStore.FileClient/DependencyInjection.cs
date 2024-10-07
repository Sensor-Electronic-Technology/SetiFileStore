using Microsoft.Extensions.DependencyInjection;

namespace SetiFileStore.FileClient;

public static class DependencyInjection {
    public static IServiceCollection AddSetiFileClient(this IServiceCollection services) {
        services.AddScoped<FileService>();
        return services;
    }
}