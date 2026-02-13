using System.Diagnostics.CodeAnalysis;
using Content.Shared.Preferences.Loadouts;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts.Effects;

[ImplicitDataDefinitionForInheritors]
public abstract partial class LoadoutEffect
{
    /// <summary>
    /// Tries to validate the effect.
    /// </summary>
    /// <param name="proto">The loadout prototype being validated.</param>
    /// <param name="sponsorPrototypes">Optional. When provided, used to filter sponsor-only loadouts. Null = allow all (backward compatible).</param>
    public abstract bool Validate(
        HumanoidCharacterProfile profile,
        RoleLoadout loadout,
        LoadoutPrototype proto,
        ICommonSession? session,
        IDependencyCollection collection,
        IReadOnlyCollection<string>? sponsorPrototypes,
        [NotNullWhen(false)] out FormattedMessage? reason);

    public virtual void Apply(RoleLoadout loadout) {}
}
