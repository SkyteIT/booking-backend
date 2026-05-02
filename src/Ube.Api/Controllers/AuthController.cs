using Microsoft.AspNetCore.Mvc;
using Ube.Application.Common.Interfaces.Services.Auth;
using Ube.Application.Features.Auth;

namespace Ube.API.Controllers;

/// <summary>
/// Authentication Controller - Handles user registration, login, and email verification
/// 
/// AUTHENTICATION FLOW:
/// ═════════════════════════════════════════════════════════════════════════════════
/// 
/// 1. REGISTRATION (POST /api/auth/register)
///    └─→ Creates new user → Returns { token, userId, email, role }
/// 
/// 2. LOGIN (POST /api/auth/login)
///    └─→ Authenticates user → Returns { token, userId, email, role }
///    └─→ Token is JWT containing user claims (userId, email, role)
/// 
/// 3. SUBSEQUENT REQUESTS (All other endpoints)
///    └─→ Client includes token in Authorization header: "Bearer {token}"
///    └─→ Middleware extracts userId from token claims
///    └─→ ICurrentUserService.UserId provides the authenticated user's ID
/// 
/// 4. GET CURRENT USER (GET /api/auth/current-user)
///    └─→ Requires: Authorization header with Bearer token
///    └─→ Returns: { userId } from ICurrentUserService.UserId
///    └─→ This userId comes from the JWT token in the request
/// 
/// 5. VERIFY EMAIL (POST /api/auth/verify-email)
///    └─→ Takes email verification token from query parameter
///    └─→ Updates user's IsEmailVerified flag
/// 
/// TOKEN USAGE IN OTHER ENDPOINTS:
/// ═════════════════════════════════════════════════════════════════════════════════
/// 
/// After login, the token is used in ALL protected endpoints:
/// 
/// Example with Bookings Controller:
///   Request: POST /api/bookings/create
///   Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }
///   
///   Middleware Flow:
///   1. Extracts token from Authorization header
///   2. Validates JWT signature and expiration
///   3. Extracts userId claim from token payload
///   4. Stores userId in ICurrentUserService
///   5. Controller method calls _currentUserService.UserId
///   6. Gets authenticated user ID without passing it in body
/// 
/// SEED DATA MAPPING:
/// ═════════════════════════════════════════════════════════════════════════════════
/// - register:      No pre-seeded data (creates new user)
/// - login:         TestDataSeeder.AuthTestLoginUserId (testlogin@example.com)
/// - verify-email:  TestDataSeeder.AuthTestVerifyEmailUserId (testverify@example.com)
/// - current-user:  TestDataSeeder.AuthTestCurrentUserUserId (testcurrentuser@example.com)
/// 
/// TESTING WORKFLOW:
/// ═════════════════════════════════════════════════════════════════════════════════
/// 
/// Step 1: Login
///   POST /api/auth/login
///   {
///     "email": "testlogin@example.com",
///     "password": "SecurePassword123!"
///   }
///   Response: { "token": "eyJhbGciOiJIUzI1NiIs...", "userId": "aaaaaaaa-1111..." }
/// 
/// Step 2: Use token in subsequent request
///   GET /api/auth/current-user
///   Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }
///   Response: { "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa" }
/// 
/// Step 3: Use same token in other endpoints (e.g., create booking)
///   POST /api/bookings/create
///   Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }
///   Body: { "listingId": "...", "startDate": "...", "endDate": "..." }
///   → Endpoint automatically knows current user from token
/// </summary>
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUserService;

    public AuthController(IAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Register a new user
    /// 
    /// Seed Data: N/A (no pre-seeded data needed)
    /// 
    /// Test Request:
    /// POST /api/auth/register
    /// {
    ///   "email": "newuser@example.com",
    ///   "password": "SecurePassword123!",
    ///   "firstName": "John",
    ///   "lastName": "Doe"
    /// }
    /// 
    /// Response:
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
    ///   "email": "newuser@example.com",
    ///   "role": "User",
    ///   "verificationToken": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
    /// }
    /// 
    /// IMPORTANT - Two different tokens are returned:
    /// 1. "token" (JWT) - Use this in Authorization header for future requests
    /// 2. "verificationToken" - Use this to verify email immediately: 
    ///    POST /api/auth/verify-email?token={verificationToken}
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password
    /// 
    /// Seed Data: TestDataSeeder.AuthTestLoginUserId
    /// Email: testlogin@example.com
    /// Password: SecurePassword123!
    /// 
    /// Test Request:
    /// POST /api/auth/login
    /// {
    ///   "email": "testlogin@example.com",
    ///   "password": "SecurePassword123!"
    /// }
    /// 
    /// Response:
    /// {
    ///   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa",
    ///   "email": "testlogin@example.com",
    ///   "role": "User"
    /// }
    /// 
    /// Next Steps:
    /// 1. Store the token on client side
    /// 2. Include token in Authorization header: "Bearer {token}"
    /// 3. Use for all subsequent authenticated requests
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request)
    {
        var result = await _authService.LoginAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Verify user email with verification token
    /// 
    /// Seed Data: TestDataSeeder.AuthTestVerifyEmailUserId
    /// Email: testverify@example.com
    /// Current Status: IsEmailVerified = false
    /// 
    /// Test Request:
    /// POST /api/auth/verify-email?token=your-verification-token
    /// 
    /// Response:
    /// {
    ///   "message": "Email verified successfully"
    /// }
    /// 
    /// Note: This endpoint typically doesn't require authentication because
    /// the verification token is sent via email link
    /// </summary>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        await _authService.VerifyEmailAsync(token);
        return Ok(new { message = "Email verified successfully" });
    }

    /// <summary>
    /// Get current authenticated user's information
    /// 
    /// Seed Data: TestDataSeeder.AuthTestCurrentUserUserId
    /// Email: testcurrentuser@example.com
    /// UserId: aaaaaaaa-3333-3333-3333-aaaaaaaaaaaa
    /// 
    /// Auth Required: YES - Token must be in Authorization header
    /// 
    /// How it works:
    /// 1. Client sends request with Authorization header
    /// 2. Middleware extracts userId from JWT token
    /// 3. ICurrentUserService.UserId is populated with the authenticated user's ID
    /// 4. This endpoint returns that userId
    /// 
    /// Test Request:
    /// GET /api/auth/current-user
    /// Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }
    /// 
    /// Response:
    /// {
    ///   "userId": "aaaaaaaa-3333-3333-3333-aaaaaaaaaaaa"
    /// }
    /// 
    /// CONNECTION TO OTHER ENDPOINTS:
    /// All other endpoints (Bookings, Listings, etc.) use the same pattern:
    /// 1. Require Authorization header with Bearer token
    /// 2. Call _currentUserService.UserId to get the authenticated user
    /// 3. Filter/create data for that specific user
    /// 
    /// Example in Bookings Controller:
    ///   public async Task CreateBooking(CreateBookingRequest request)
    ///   {
    ///       var currentUserId = _currentUserService.UserId;  // Gets from token
    ///       // Create booking for currentUserId
    ///   }
    /// </summary>
    [HttpGet("current-user")]
    public IActionResult GetCurrentUser()
    {
        var userId = _currentUserService.UserId;
        return Ok(new { userId });
    }
}