namespace Product.Domain;

/// <summary>What kind of digital good a listing is — drives what "buying it" delivers (see OrderItem.AccessKey/AssetUrl in Order Service).</summary>
public enum ProductType
{
    /// <summary>A software license key (desktop app, plugin, etc.).</summary>
    License,

    /// <summary>An API key granting access to a hosted API.</summary>
    ApiAccess,

    /// <summary>A SaaS subscription plan — see PricingModel for the billing cadence label.</summary>
    SaaSSubscription,

    /// <summary>A project/portfolio bundle (source code, template, starter kit) delivered via AssetUrl.</summary>
    Project,
}
