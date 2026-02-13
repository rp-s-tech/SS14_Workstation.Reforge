using Content.Shared.RPSX.Roles.Salary;
using Robust.Shared.Network;

namespace Content.Shared.StationRecords;

/// <summary>
/// RPSX extension: Salary, NetUserId, MobEntity for CCO/Salary integration.
/// </summary>
public sealed partial record GeneralStationRecord
{
    /// <summary>
    /// Net user ID for bank/salary operations.
    /// </summary>
    [DataField]
    public NetUserId? NetUserId;

    /// <summary>
    /// Mob entity (current body) for salary payout.
    /// </summary>
    [DataField]
    public NetEntity? MobEntity;

    /// <summary>
    /// Salary entry for CCO console and payday.
    /// </summary>
    [DataField]
    public CrewSalaryEntry? Salary;
}
