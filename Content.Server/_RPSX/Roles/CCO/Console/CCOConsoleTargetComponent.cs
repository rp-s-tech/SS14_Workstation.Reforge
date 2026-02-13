using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Server.RPSX.Roles.CCO.Console;

[RegisterComponent]
public sealed partial class CCOConsoleTargetComponent : Component
{
    [DataField]
    public bool IsSpecialSquadCalled;
}
