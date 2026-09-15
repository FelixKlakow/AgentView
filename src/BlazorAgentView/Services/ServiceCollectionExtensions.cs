using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

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
        => services.AddBlazorAgentView(null);

    /// <summary>
    /// Registers the default <see cref="IMarkdownRenderer"/> implementation with a
    /// tuned Markdig pipeline. The builder handed to <paramref name="configurePipeline"/>
    /// already has <c>UseAdvancedExtensions()</c> applied; raw HTML is disabled
    /// afterwards regardless of what the callback does.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configurePipeline">
    /// Callback used to adjust the Markdig pipeline — for example to remove
    /// extensions whose parsers are expensive or fragile on machine-generated
    /// input. Pass <c>null</c> for the default pipeline.
    /// </param>
    /// <remarks>
    /// Safe to call multiple times; existing registrations are preserved so
    /// consumers may swap in a custom renderer before calling this method.
    /// </remarks>
    public static IServiceCollection AddBlazorAgentView(
        this IServiceCollection services,
        Action<MarkdownPipelineBuilder>? configurePipeline)
    {
        var pipeline = configurePipeline is null ? null : DefaultMarkdownRenderer.CreatePipeline(configurePipeline);

        services.TryAddSingleton<IMarkdownRenderer>(sp => new DefaultMarkdownRenderer(
            pipeline,
            sp.GetService<ILogger<DefaultMarkdownRenderer>>()));

        return services;
    }
}
