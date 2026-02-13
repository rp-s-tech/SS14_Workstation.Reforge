using System.Collections.Generic;
using Content.Shared.NPC.Prototypes;
using Content.Shared.RPSX.Roles.CCO;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.ViewVariables;
using Color = Robust.Shared.Maths.Color;

namespace Content.Server.RPSX.Roles.CCO.Console;

[RegisterComponent]
public sealed partial class CcoConsoleComponent : Component
{
    [DataField]
    public EmergencyShuttleState EmergencyShuttleState;

    [DataField]
    public ProtoId<NpcFactionPrototype> TargetFaction = "NanoTrasen";

    [ViewVariables]
    public List<Entity<CCOConsoleTargetComponent>> AvailableStations = new();

    [DataField]
    public EntityUid? Station;

    [ViewVariables]
    [DataField(required: true)]
    public string AnnouncementDisplayName = "ОЦК";

    [ViewVariables]
    [DataField]
    public Color AnnouncementColor = Color.YellowGreen;
}
