namespace Product.Domain;

/// <summary>
/// How a product is billed. This is a display label only — there is no recurring billing engine
/// behind Monthly/Yearly (a known, deliberate simplification; see README). Every purchase is a
/// single one-time payment regardless of PricingModel.
/// </summary>
public enum PricingModel
{
    OneTime,
    Monthly,
    Yearly,
}
