using System.Diagnostics.CodeAnalysis;
using Content.Server.Database;
using Content.Server.Mind;
using Content.Server.Preferences.Managers;
using Content.Server.Stack;
using Content.Server.StationRecords.Systems;
using Content.Shared.Coordinates;
using Content.Shared.Mind;
using Content.Shared.PDA;
using Content.Shared.RPSX.Bank.Components;
using Content.Shared.RPSX.Bank.Systems;
using Content.Shared.StationRecords;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server.RPSX.Bank;

public sealed partial class BankSystem : SharedBankSystem
{
    [Dependency] private readonly StationRecordsSystem _stationRecordsSystem = default!;
    [Dependency] private readonly IServerPreferencesManager _prefsManager = default!;
    [Dependency] private readonly IServerDbManager _dbManager = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;

    private ISawmill _log = default!;

    public override void Initialize()
    {
        base.Initialize();
        _log = Logger.GetSawmill("bank");
        InitializeAccount();

        SubscribeLocalEvent<BankATMComponent, SpawnCashEvent>(SpawnCash);
    }

    public async void RefreshBankBalanceAsync(Entity<MindComponent?> mind)
    {
        if (!Resolve(mind, ref mind.Comp))
            return;

        if (mind.Comp.UserId == null)
            return;

        var userId = mind.Comp.UserId.Value;

        if (!_prefsManager.TryGetCachedPreferences(userId, out var prefs))
            return;

        var character = prefs.SelectedCharacter;
        var index = prefs.IndexOfCharacter(character);

        if (character is not Content.Shared.Preferences.HumanoidCharacterProfile)
            return;

        if (!TryComp<BankAccountComponent>(mind, out var bank))
            return;

        var account = await _dbManager.GetProfileEconomics(userId, index);
        if (account == null)
            return;

        bank.Balance = account.Balance;
        Dirty(mind, bank);
    }

    public async void UpdateProfile(NetUserId userId, BankAccountComponent bank, int index)
    {
        await _dbManager.SaveProfileEconomics(userId, index, bank);
    }

    public bool TryGetGeneralStationRecordAndStation(EntityUid pda, [NotNullWhen(true)] out GeneralStationRecord? record,
        [NotNullWhen(true)] out EntityUid? station)
    {
        record = null;
        station = null;

        if (!TryComp<PdaComponent>(pda, out var comp) || comp.ContainedId == null)
            return false;

        if (!TryComp<StationRecordKeyStorageComponent>(comp.ContainedId, out var keyStorage) || keyStorage.Key == null)
            return false;

        station = keyStorage.Key.Value.OriginStation;

        if (!_stationRecordsSystem.TryGetRecord(keyStorage.Key.Value, out record))
            return false;

        _stationRecordsSystem.Synchronize(station.Value);
        return true;
    }

    private void SpawnCash(EntityUid uid, BankATMComponent bankATM, SpawnCashEvent args)
    {
        var stacks = _stackSystem.SpawnMultipleAtPosition(new EntProtoId(args.CashType), args.Amount, GetEntity(args.Where).ToCoordinates());
        foreach (var stack in stacks)
        {
            RemComp<BankSecureCashComponent>(stack);
        }
    }
}
