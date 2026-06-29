using Microsoft.Extensions.DependencyInjection;
using TFTCoach.Capture.Services;
using TFTCoach.Core.Interfaces;
using TFTCoach.Infrastructure.Services;

namespace TFTCoach.App.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTftCoach(this IServiceCollection services)
    {
        services.AddSingleton<IProcessService, TftProcessService>();
        services.AddSingleton<ICaptureService, CaptureService>();

        return services;
    }
}