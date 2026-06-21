using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DeepSearch.Application;

/// <summary>
/// Registers the Application layer: MediatR handlers, FluentValidation validators,
/// and the cross-cutting validation pipeline behavior.
/// Keeping registration here preserves the Clean Architecture dependency rule
/// (the Api project does not need to know about Application internals).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(Behaviors.ValidationBehavior<,>));
        services.AddSingleton<Services.QuestionPhrasingService>();

        return services;
    }
}
