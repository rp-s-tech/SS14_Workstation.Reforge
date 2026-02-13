using Robust.Shared.Prototypes;

namespace Content.Shared.RPSX.Sponsors;

[Prototype("sponsorsItemsCategory")]
public sealed partial class SponsorsItemsCategory : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = default!;

    [DataField]
    public List<EntProtoId> Items = new();
}
