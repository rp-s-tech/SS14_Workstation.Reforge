using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Preferences.Loadouts;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

/// <summary>
/// Only sponsors that have this loadout in their allowed list can select it.
/// When sponsorPrototypes is null (not passed), all users are allowed (backward compatible).
/// </summary>
public sealed partial class SponsorLoadoutEffect : LoadoutEffect
{
    public override bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        LoadoutPrototype proto,
        ICommonSession? session,
        IDependencyCollection collection,
        IReadOnlyCollection<string>? sponsorPrototypes,
        [NotNullWhen(false)] out FormattedMessage? reason)
    {
        reason = null;

        // Backward compatibility: when sponsor-prototypes not passed, allow all
        if (sponsorPrototypes == null)
            return true;

        if (session == null)
            return true;

        if (sponsorPrototypes.Contains(proto.ID))
            return true;

        reason = FormattedMessage.FromMarkupOrThrow(Loc.GetString("loadout-sponsor-only"));
        return false;
    }
}
