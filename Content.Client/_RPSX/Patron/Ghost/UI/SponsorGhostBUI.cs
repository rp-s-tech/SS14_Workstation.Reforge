using Content.Shared.RPSX.Patron.Ghost;
using Robust.Shared.GameObjects;

namespace Content.Client.RPSX.Patron.Ghost.UI;

public sealed class SponsorGhostBUI : BoundUserInterface
{
    private SponsorGhostWindow? _window;

    public SponsorGhostBUI(EntityUid owner, Enum uiKey) : base(owner, uiKey) {}

    protected override void Open()
    {
        base.Open();
        _window = new SponsorGhostWindow();
        _window.OnClose += Close;
        _window.OpenCentered();

        _window.OnSelectGhost += id => SendMessage(new SponsorChangeGhostEvent(id));
        _window.PopulateItems();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Dispose();
        }
    }
}
