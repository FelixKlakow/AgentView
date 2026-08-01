using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlazorAgentView.Services;

/// <summary>
/// Convenience DI registrations for the BlazorAgentView component library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default <see cref="IMarkdownRenderer"/> implementation.
    /// Safe to call multiple times; existing registrations are preserved so
    /// consumers may swap in a custom renderer before calling this method.
    /// </summary>
    public static IServiceCollection AddBlazorAgentView(this IServiceCollection services)
    {
        services.TryAddSingleton<IMarkdownRenderer, DefaultMarkdownRenderer>();
        return services;
    }
}
