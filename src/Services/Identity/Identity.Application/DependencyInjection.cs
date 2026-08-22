using ECommerce.BuildingBlocks.Application.Behaviors;
using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityApplication(this IServiceCollection services)
    {
        services.AddMediator(typeof(DependencyInjection).Assembly);
        services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        return services;
    }
}
