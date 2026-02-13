using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.RPSX.Administration.Commands.ERT;

[AdminCommand(AdminFlags.Round)]
public sealed class ERTSpawnDropCommand : IConsoleCommand
{
    public string Command => "ertspawndrop";
    public string Description => "Resets ertspawn status to allow another team call.";
    public string Help => "Usage: ertspawndrop";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var ertSystem = entityManager.System<ERTSystem>();

        if (shell.Player is null)
            return;

        ertSystem.DropStatus();
    }
}
