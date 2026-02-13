using Content.Shared.Cargo.Components;
using Content.Shared.Examine;
using Content.Shared.RPSX.Bank.Components;
using Content.Shared.Stacks;

namespace Content.Shared.RPSX.Bank.Systems;

public sealed partial class BankSecuritySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        // Target API does not expose stack merge attempt events used upstream.
        // Keep split + examine handling to preserve secure cash marker behavior.
        SubscribeLocalEvent<CashComponent, StackSplitEvent>(OnStackSplitEvent);
        SubscribeLocalEvent<BankSecureCashComponent, ExaminedEvent>(OnExaminedEvent);
    }

    private void OnExaminedEvent(EntityUid uid, BankSecureCashComponent component, ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        args.PushMarkup(Loc.GetString("bank-secure-cash-markup"));
    }

    private void OnStackSplitEvent(EntityUid uid, CashComponent component, ref StackSplitEvent args)
    {
        if (HasComp<BankSecureCashComponent>(uid))
            EnsureComp<BankSecureCashComponent>(args.NewId);
        else
            RemComp<BankSecureCashComponent>(args.NewId);
    }
}
