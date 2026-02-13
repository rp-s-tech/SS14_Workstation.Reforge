using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class RPSXCCVars : CVars
{
    /// <summary>
    ///     Enable discord verify gate for connection.
    /// </summary>
    public static readonly CVarDef<bool> DiscordAuthEnabled =
        CVarDef.Create("discord.auth_enabled", false, CVar.NOTIFY | CVar.REPLICATED);

    /// <summary>
    ///     Discord verify server URL.
    /// </summary>
    public static readonly CVarDef<string> DiscordAuthServer =
        CVarDef.Create("discord.api_url", "", CVar.SERVERONLY);
}
