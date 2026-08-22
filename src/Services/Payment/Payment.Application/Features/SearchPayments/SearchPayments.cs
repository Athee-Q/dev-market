using ECommerce.BuildingBlocks.Application.Mediator;
using ECommerce.Contracts.Common;
using Payment.Application.Abstractions;
using Payment.Application.Dto;
using Payment.Domain;

namespace Payment.Application.Features.SearchPayments;

/// <summary>The Transaction History page's backing query — a customer's own payments, or (with PaymentsManage) anyone's.</summary>
public static class SearchPayments
{
    public record Query(Guid? CustomerId, PaymentStatus? Status, int Page = 1, int PageSize = 20) : IRequest<PagedResult<PaymentDto>>;

    public class Handler(IPaymentRepository repository, IPaymentGateway gateway) : IRequestHandler<Query, PagedResult<PaymentDto>>
    {
        public async Task<PagedResult<PaymentDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var (items, total) = await repository.SearchAsync(request.CustomerId, request.Status, request.Page, request.PageSize, cancellationToken);
            var dtos = items.Select(p => PaymentDto.FromDomain(p, gateway.KeyId)).ToList();
            return new PagedResult<PaymentDto>(dtos, request.Page, request.PageSize, total);
        }
    }
}
