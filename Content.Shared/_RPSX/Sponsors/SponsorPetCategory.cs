using Robust.Shared.Prototypes;

namespace Content.Shared.RPSX.Sponsors;

[Prototype("sponsorPetCategory")]
public sealed partial class SponsorPetCategory : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<EntProtoId> Pets = [];
}
