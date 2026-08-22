namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>
/// Marks a request (a vertical-slice Command or Query) that returns <typeparamref name="TResponse"/>.
/// Implemented as an empty marker, same shape as the well-known MediatR contract, so this reads
/// familiarly — but this whole folder is an independent, from-scratch implementation with no
/// external dependency (MediatR itself went commercial-only at v13; a hand-rolled mediator sidesteps
/// that entirely for a learning project).
/// </summary>
public interface IRequest<out TResponse>;
