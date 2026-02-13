using Robust.Shared.Prototypes;

namespace Content.Client.RPSX.Entry;

/// <summary>
/// Registers RPSX-specific prototype ignores on client (server-only prototypes).
/// </summary>
public sealed class RPSXRegisterIgnore
{
    public void RegisterIgnore(IPrototypeManager prototypeManager)
    {
        prototypeManager.RegisterIgnore("ERTGroup"); // RPSX - server-only
        prototypeManager.RegisterIgnore("salaries"); // RPSX - server-only
    }
}
