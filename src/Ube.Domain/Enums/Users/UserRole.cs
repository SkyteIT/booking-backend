namespace Ube.Domain.Enums.Users;

public enum UserRole
{
    User = 0,
    Vendor = 1,
    Admin = 2,

    // Narrower than Admin - can act on money-movement endpoints (payout
    // exports, batch settlement, vendor invoices) but has no general
    // admin capability (user/vendor/category management). Assigned via
    // the existing PUT /api/admin/users/{userId}/role endpoint - no new
    // endpoint needed since UserRole is already the parameter type there.
    Finance = 3,

    // Broader than Admin, not narrower - can read everything an Admin or
    // Finance user can (TokenService grants SuperAdmin's JWT all three
    // role claims), but role-change actions initiated by a plain Admin
    // don't take effect immediately - they land in a RoleChangeRequest
    // queue only a SuperAdmin can approve or reject. See
    // Features/Users/RoleChangeRules.cs.
    SuperAdmin = 4
}