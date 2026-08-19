using Ube.Domain.Entities.Users;
using Ube.Domain.Enums.Users;

namespace Ube.Application.Features.Users;

// Guards applied wherever a role change actually takes effect - a
// SuperAdmin's direct change, or an approval of a pending
// RoleChangeRequest. Never applied at request-creation time, since a
// request never mutates anything on its own.
public static class RoleChangeRules
{
    public static Result CanChangeRole(Guid actorUserId, User target, UserRole newRole, int adminOrSuperAdminCount)
    {
        if (target.Id == actorUserId)
            return Result.Failure("You cannot change your own role.");

        var targetHoldsAdminPower = target.Role is UserRole.Admin or UserRole.SuperAdmin;
        var newRoleHoldsAdminPower = newRole is UserRole.Admin or UserRole.SuperAdmin;

        if (targetHoldsAdminPower && !newRoleHoldsAdminPower && adminOrSuperAdminCount <= 1)
            return Result.Failure("Cannot remove the last remaining Admin/SuperAdmin - promote another user first.");

        return Result.Success();
    }

    // Suspension is a lighter action than a role change (fully reversible,
    // no data mutation beyond a flag), but a plain Admin suspending a
    // SuperAdmin is still a real privilege-escalation path - the
    // suspended SuperAdmin can no longer act, while the suspending Admin
    // remains fully powered. Same hierarchy protection as role changes,
    // applied to status instead.
    public static Result CanChangeStatus(Guid actorUserId, User target, UserRole actorRole)
    {
        if (target.Id == actorUserId)
            return Result.Failure("You cannot suspend your own account.");

        if (target.Role == UserRole.SuperAdmin && actorRole != UserRole.SuperAdmin)
            return Result.Failure("Only a SuperAdmin can suspend a SuperAdmin account.");

        return Result.Success();
    }
}
