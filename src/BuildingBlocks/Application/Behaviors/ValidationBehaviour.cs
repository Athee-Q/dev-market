using ECommerce.BuildingBlocks.Application.Mediator;
using FluentValidation;

namespace ECommerce.BuildingBlocks.Application.Behaviors;

/// <summary>
/// Runs every registered FluentValidation validator for TRequest before the handler executes,
/// replacing the old MVC-controller pattern of each action manually calling
/// `validator.ValidateAndThrowAsync(...)`. Throws the same FluentValidation.ValidationException
/// the shared AppExceptionHandler already maps to 400, so behavior for callers is unchanged.
/// </summary>
public class ValidationBehaviour<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next();
    }
}
