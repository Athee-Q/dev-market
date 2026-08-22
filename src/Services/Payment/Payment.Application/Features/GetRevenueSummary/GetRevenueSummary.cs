using ECommerce.BuildingBlocks.Application.Mediator;
using Payment.Application.Abstractions;

namespace Payment.Application.Features.GetRevenueSummary;

/// <summary>Admin dashboard KPI tile — see Permissions.PaymentsManage, enforced at the endpoint.</summary>
public static class GetRevenueSummary
{
    public record Query : IRequest<Result>;

    public record Result(decimal TotalRevenue, int SucceededPaymentCount);

    public class Handler(IPaymentRepository repository) : IRequestHandler<Query, Result>
    {
        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var (totalRevenue, succeededCount) = await repository.GetRevenueSummaryAsync(cancellationToken);
            return new Result(totalRevenue, succeededCount);
        }
    }
}
