namespace ECommerce.BuildingBlocks.Application.Mediator;

/// <summary>Stand-in "no meaningful response" type for commands that only need to run, not return a value.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value = default;
}
