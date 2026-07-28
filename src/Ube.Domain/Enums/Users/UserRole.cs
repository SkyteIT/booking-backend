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
    Finance = 3
}