using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Robust.Shared.Player;

namespace Content.Server.RPSX.Utils;

public static class ChatUtils
{
    public static void SendMessageFromCentcom(
        ChatSystem chatSystem,
        string message,
        string sender,
        EntityUid? stationId)
    {
        if (stationId == null)
        {
            chatSystem.DispatchGlobalAnnouncement(message, sender, playSound: true);
        }
        else
        {
            chatSystem.DispatchStationAnnouncement(stationId.Value, message, sender, playDefaultSound: true);
        }
    }
}
