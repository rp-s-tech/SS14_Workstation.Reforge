using System.Collections.Generic;
using System.Linq;
using Content.Server.RPSX.Roles.Salary.Systems;
using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.CrewManifest;
using Content.Server.RoundEnd;
using Content.Server.RPSX.Administration.Commands.ERT;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.RPSX.FastUI;
using Content.Shared.RPSX.Roles.CCO;
using Content.Shared.Station.Components;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Robust.Shared.Log;

namespace Content.Server.RPSX.Roles.CCO.Console;

public sealed class CcoConsoleSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly CrewManifestSystem _crewManifestSystem = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttleSystem = default!;
    [Dependency] private readonly ERTSystem _ertSystem = default!;
    [Dependency] private readonly NpcFactionSystem _faction = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly CrewMemberSalarySystem _crewMemberSalarySystem = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    private readonly List<SecretListingPrototype> ertGroupPrototypes = new();

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("cco-console");

        Subs.BuiEvents<CcoConsoleComponent>(CcoConsoleInterfaceKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnUIOpened);
            subs.Event<CcoConsoleSendAnnouncementMessage>(OnAnnouncement);
            subs.Event<CcoConsoleSendEmergencyShuttleMessage>(OnEmergencyShuttle);
            subs.Event<CcoConsoleCancelEmergencyShuttleMessage>(OnEmergencyShuttleCancelled);
            subs.Event<CcoConsoleSendSpecialSquadMessage>(OnSpecialSquadSend);
            subs.Event<CcoConsoleOpenCrewManifestMessage>(OnOpenManifest);
            subs.Event<CcoConsoleStationSelected>(OnStationSelected);

            subs.Event<CcoConsoleCrewMemberSalaryBonusMessage>(OnCrewMemberBonus);
            subs.Event<CcoConsoleCrewMemberSalaryPenaltyMessage>(OnCrewMemberPenalty);
        });

        SubscribeLocalEvent<CcoConsoleComponent, AlertLevelChangedEvent>(OnAlertLevelChanged);
        SubscribeLocalEvent<RoundEndSystemChangedEvent>(OnRoundEndSystemChanged);

        var ertListing = _prototype.Index<SecretListingCategoryPrototype>("ERTGroupsListing");
        ertGroupPrototypes.AddRange(ertListing.Listings.Select(id => _prototype.Index<SecretListingPrototype>(id)));
    }

    private void OnAnnouncement(Entity<CcoConsoleComponent> ent, ref CcoConsoleSendAnnouncementMessage args)
    {
        SendMessage(ent, args.Actor, args.Message);
    }

    private void SendMessage(Entity<CcoConsoleComponent> ent, EntityUid user, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _sawmill.Warning("Attempted to send an empty announcement.");
            return;
        }

        Loc.TryGetString(ent.Comp.AnnouncementDisplayName, out var annTitle);
        annTitle ??= "ОЦК";

        var sender = $"{annTitle} {Name(user)}";
        var stationId = ent.Owner;

        if (!TryComp<StationDataComponent>(stationId, out _))
        {
            _sawmill.Warning($"Invalid station ID: {stationId}. Sending global announcement.");
            _chatSystem.DispatchGlobalAnnouncement(
                message: message,
                sender: sender,
                playSound: true,
                colorOverride: ent.Comp.AnnouncementColor
            );
            return;
        }

        _chatSystem.DispatchStationAnnouncement(
            source: stationId,
            message: message,
            sender: sender,
            playDefaultSound: true,
            colorOverride: ent.Comp.AnnouncementColor
        );

        _sawmill.Info($"Announcement sent: {message} by {sender}");
    }

    private void OnStationSelected(Entity<CcoConsoleComponent> ent, ref CcoConsoleStationSelected args)
    {
        var station = GetEntity(args.Station);
        ent.Comp.Station = station;

        UpdateCcoConsoleState(ent, args.Actor);
    }

    private void OnCrewMemberBonus(Entity<CcoConsoleComponent> ent, ref CcoConsoleCrewMemberSalaryBonusMessage args)
    {
        if (ent.Comp.Station is not { } station)
            return;

        _crewMemberSalarySystem.UpdateCrewMemberBonus(station, args.Actor, args.Bonus, args.Record);
        UpdateCcoConsoleState(ent, args.Actor);
    }

    private void OnCrewMemberPenalty(Entity<CcoConsoleComponent> ent, ref CcoConsoleCrewMemberSalaryPenaltyMessage args)
    {
        if (ent.Comp.Station is not { } station)
            return;

        _crewMemberSalarySystem.UpdateCrewMemberPenalty(station, args.Actor, args.Penalty, args.Record);
        UpdateCcoConsoleState(ent, args.Actor);
    }

    private void OnOpenManifest(Entity<CcoConsoleComponent> ent, ref CcoConsoleOpenCrewManifestMessage args)
    {
        if (ent.Comp.Station is not { } station)
            return;

        if (!TryComp<ActorComponent>(args.Actor, out var actor))
            return;

        _crewManifestSystem.OpenEui(station, actor.PlayerSession);
    }

    private void OnRoundEndSystemChanged(RoundEndSystemChangedEvent args)
    {
        var query = EntityQueryEnumerator<CcoConsoleComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            var ent = (uid, component);
            UpdateEmergencyShuttleState(ent);

            var userEntity = _ui.GetActors(uid, CcoConsoleInterfaceKey.Key).FirstOrNull();
            if (userEntity == null)
                continue;

            UpdateCcoConsoleState(ent, userEntity.Value);
        }
    }

    private void OnSpecialSquadSend(Entity<CcoConsoleComponent> ent, ref CcoConsoleSendSpecialSquadMessage args)
    {
        if (ent.Comp.Station is not { } station)
            return;

        if (!TryComp<CCOConsoleTargetComponent>(station, out var ccoConsoleTargetComponent))
            return;

        if (ccoConsoleTargetComponent.IsSpecialSquadCalled)
            return;

        ccoConsoleTargetComponent.IsSpecialSquadCalled = true;

        _ertSystem.SpawnERT(args.SquadId);

        SendMessage(ent, args.Actor, "Специальный отряд был отправлен на станцию");
        UpdateCcoConsoleState(ent, args.Actor);
    }

    private void OnEmergencyShuttleCancelled(Entity<CcoConsoleComponent> ent,
        ref CcoConsoleCancelEmergencyShuttleMessage args)
    {
        _roundEndSystem.CancelRoundEndCountdown();

        UpdateEmergencyShuttleState(ent);

        if (!_ui.HasUi(ent, CcoConsoleInterfaceKey.Key))
            return;

        var userEntity = _ui.GetActors(ent.Owner, CcoConsoleInterfaceKey.Key).FirstOrDefault();
        UpdateCcoConsoleState(ent, userEntity);
    }

    private void OnEmergencyShuttle(Entity<CcoConsoleComponent> ent, ref CcoConsoleSendEmergencyShuttleMessage args)
    {
        _roundEndSystem.RequestRoundEnd(ent.Comp.Station);
        UpdateEmergencyShuttleState(ent);
        UpdateCcoConsoleState(ent, args.Actor);

        SendMessage(ent, args.Actor, "Эвакуационный шаттл был отправлен");
    }

    private void UpdateEmergencyShuttleState(Entity<CcoConsoleComponent> ent)
    {
        EmergencyShuttleState state;

        if (_emergencyShuttleSystem.EmergencyShuttleArrived)
            state = EmergencyShuttleState.Arrived;
        else if (_emergencyShuttleSystem.ShuttlesLeft)
            state = EmergencyShuttleState.OnWayCentcom;
        else if (_roundEndSystem.ExpectedCountdownEnd != null)
            state = EmergencyShuttleState.OnWay;
        else if (!_roundEndSystem.CanCallOrRecall())
            state = EmergencyShuttleState.Unknown;
        else
            state = EmergencyShuttleState.Idle;

        ent.Comp.EmergencyShuttleState = state;
    }

    private void OnAlertLevelChanged(Entity<CcoConsoleComponent> ent, ref AlertLevelChangedEvent args)
    {
        if (!_ui.HasUi(ent, CcoConsoleInterfaceKey.Key))
            return;

        var userEntity = _ui.GetActors(ent.Owner, CcoConsoleInterfaceKey.Key).FirstOrDefault();
        UpdateCcoConsoleState(ent, userEntity);
    }

    private void OnUIOpened(Entity<CcoConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        OpenCcoConsoleWindow(ent, args.Actor);
    }

    private void OpenCcoConsoleWindow(Entity<CcoConsoleComponent> ent, EntityUid user)
    {
        if (!_ui.HasUi(ent, CcoConsoleInterfaceKey.Key))
            return;

        UpdateCcoConsoleState(ent, user);
    }

    private void UpdateCcoConsoleState(Entity<CcoConsoleComponent> ent, EntityUid user)
    {
        EnsureStations(ent);

        var stationsStates = GetStationsState(ent);
        var baseConsole = GetBaseConsoleState(ent, user);
        var ccoState = new CcoConsoleUIState(
            stationsStates,
            baseConsole
        );

        _ui.SetUiState(
            ent.Owner,
            CcoConsoleInterfaceKey.Key,
            ccoState
        );
    }

    private CcoConsoleBase GetBaseConsoleState(Entity<CcoConsoleComponent> ent, EntityUid user)
    {
        var operatorName = MetaData(user).EntityName;
        return new CcoConsoleBase(
            GetNetEntity(ent.Comp.Station),
            operatorName
        );
    }

    private CcoConsoleAvailableStations GetStationsState(Entity<CcoConsoleComponent> ent)
    {
        var states = new Dictionary<NetEntity, CcoConsoleStationState>();
        foreach (var station in ent.Comp.AvailableStations)
        {
            var state = new CcoConsoleStationState(
                GetAlertState(station),
                GetEmergencyShuttleState(ent),
                GetErtGroups(station),
                GetSalaries(station),
                MetaData(station).EntityName
            );

            states[GetNetEntity(station)] = state;
        }

        return new CcoConsoleAvailableStations(states);
    }

    private void EnsureStations(Entity<CcoConsoleComponent> ent)
    {
        var availableStations = ent.Comp.AvailableStations;
        availableStations.Clear();

        var stations = _station.GetStationsSet();
        foreach (var station in stations)
        {
            if (!_faction.IsMember(station, ent.Comp.TargetFaction))
                continue;

            if (!TryComp<CCOConsoleTargetComponent>(station, out var consoleTarget))
                continue;

            availableStations.Add((station, consoleTarget));
        }

        if (ent.Comp.Station is not { } selectedStation)
            return;

        if (!availableStations.Select(station => station.Owner).Contains(selectedStation))
            ent.Comp.Station = null;
    }

    private CcoConsoleSpecialSquadModel GetErtGroups(Entity<CCOConsoleTargetComponent> station)
    {
        var specialSquads = new List<CcoConsoleSpecialSquad>();
        foreach (var prototype in ertGroupPrototypes)
        {
            var squad = new CcoConsoleSpecialSquad(
                prototype.ID,
                prototype.Title,
                prototype.Description
            );
            specialSquads.Add(squad);
        }

        return new CcoConsoleSpecialSquadModel(specialSquads, station.Comp.IsSpecialSquadCalled);
    }

    private CcoConsoleSalaries GetSalaries(EntityUid station)
    {
        var entries = _crewMemberSalarySystem.GetStationCrewEntries(station);
        return new CcoConsoleSalaries(entries);
    }

    private CcoConsoleEmergencyShuttle GetEmergencyShuttleState(Entity<CcoConsoleComponent> ent)
    {
        return new CcoConsoleEmergencyShuttle(ent.Comp.EmergencyShuttleState, _roundEndSystem.ExpectedCountdownEnd);
    }

    private CcoConsoleAlert GetAlertState(EntityUid station)
    {
        if (!TryComp<AlertLevelComponent>(station, out var component) ||
            component.AlertLevels?.Levels is not { } levels)
            return new CcoConsoleAlert("Неизвестно", Color.White, "");

        var alertLevelCode = component.CurrentLevel;
        var alertLevel = Loc.GetString($"alert-level-{alertLevelCode}");
        if (!levels.TryGetValue(alertLevelCode, out var details))
            return new CcoConsoleAlert(alertLevel, Color.White, "");

        var instructions = Loc.GetString($"alert-level-{alertLevelCode}-instructions");

        return new CcoConsoleAlert(alertLevel, details.Color, instructions);
    }
}
