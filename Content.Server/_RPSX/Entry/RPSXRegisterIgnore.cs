using System.Collections.Generic;
using Robust.Shared.ContentPack;
using Robust.Shared.GameObjects;

namespace Content.Server.RPSX.Entry;

public sealed class RPSXRegisterIgnore
{
    public void RegisterIgnore(IComponentFactory componentFactory, IResourceManager res)
    {
        var useSecrets = res.ContentFileExists("/Content.RPSX.Server.dll") ||
                         res.ContentFileExists("/Assemblies/Content.RPSX.Server.dll");

        if (!useSecrets)
        {
            // In some forks RPSX content is merged into Content.Server itself (no separate Content.RPSX.Server.dll).
            // In that case, many "secret" components are already registered, and RegisterIgnore would crash.
            // Only ignore components that are actually missing.
            var toIgnore = new List<string>();
            foreach (var name in Content.Server.Entry.IgnoredSecretComponents.List)
            {
                if (componentFactory.GetComponentAvailability(name, ignoreCase: true) != ComponentAvailability.Available)
                    toIgnore.Add(name);
            }

            if (toIgnore.Count > 0)
                componentFactory.RegisterIgnore(toIgnore.ToArray());
        }
    }
}
