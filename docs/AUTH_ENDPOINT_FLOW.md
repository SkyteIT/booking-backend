# Authentication Endpoint Flow Documentation

## Complete Authentication & Request Flow

```
┌────────────────────────────────────────────────────────────────────────────┐
│                        AUTHENTICATION FLOW DIAGRAM                         │
└────────────────────────────────────────────────────────────────────────────┘

STEP 1: USER REGISTRATION (Optional)
═════════════════════════════════════════════════════════════════════════════
POST /api/auth/register
Body:
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe"
}

Response: 
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "email": "newuser@example.com",
  "role": "User",
  "verificationToken": "af1e7ce4-93d3-4fa0-bcb8-1f3ea09bcb01"
}


STEP 1.5: VERIFY EMAIL (After Registration)
═════════════════════════════════════════════════════════════════════════════
⚠️  IMPORTANT: Use the "verificationToken" from registration response, NOT the JWT token!

POST /api/auth/verify-email?token=af1e7ce4-93d3-4fa0-bcb8-1f3ea09bcb01
(No request body needed)

Response:
{
  "message": "Email verified successfully"
}

❌ WRONG - DON'T USE THE JWT TOKEN:
POST /api/auth/verify-email?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
^ This will return 404 "Invalid token"


STEJWT TOKEN CONTAINS:
   - userId: aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa
   - email: testlogin@example.com
   - role: User
   - exp: Token expiration time
   - iss: "UbeApp"
   - aud: "UbeAppUsers"
{
  "email": "testlogin@example.com",
  "password": "SecurePassword123!"
}

Response:
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa",
  "email": "testlogin@example.com",
  "role": "User"
}

🔑 TOKEN CONTAINS:
   - userId: aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa
   - email: testlogin@example.com
   - role: User
   - exp: expiration timestamp


STEP 3: GET CURRENT USER (Verify Token Works)
═════════════════════════════════════════════════════════════════════════════
GET /api/auth/current-user
Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }

Response:
{
  "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"
}

✓ Token is valid and middleware extracted userId


STEP 4: USE TOKEN IN OTHER ENDPOINTS (All Protected Endpoints)
═════════════════════════════════════════════════════════════════════════════

Example: Create a Booking

POST /api/bookings/create
Headers: { "Authorization": "Bearer eyJhbGciOiJIUzI1NiIs..." }
Body:
{
  "listingId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "startDate": "2026-05-15",
  "endDate": "2026-05-16"
}

BEHIND THE SCENES:
1. Middleware extracts token from Authorization header
2. Validates JWT signature and expiration
3. Extracts userId claim: "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"
4. Injects into ICurrentUserService.UserId
5. Booking Controller calls: var customerId = _currentUserService.UserId
6. Creates booking with customerId = "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"
7. Returns booking with current user automatically associated

Response:
{
  "id": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee",
  "customerId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa",
  "listingId": "dddddddd-dddd-dddd-dddd-dddddddddddd",
  "status": "Pending"
}
```

## How ICurrentUserService Connects Everything

```
REQUEST FLOW:
═════════════════════════════════════════════════════════════════════════════

Client Request with Token:
    │
    ├─→ GET /api/bookings/my-bookings
    │   Headers: { "Authorization": "Bearer {token}" }
    │
    ├─→ Middleware (Authentication)
    │   ├─ Extracts token from Authorization header
    │   ├─ Validates JWT signature
    │   ├─ Extracts claims (userId, email, role)
    │   └─ Stores userId in HttpContext.User.Claims
    │
    ├─→ ICurrentUserService Implementation
    │   └─ Reads userId from HttpContext.User.Claims
    │   └─ Returns userId property
    │
    ├─→ BookingController.GetMyBookings()
    │   var currentUserId = _currentUserService.UserId
    │   // Returns: "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"
    │
    ├─→ BookingService
    │   var myBookings = await _bookingRepository
    │       .GetByCustomerIdAsync(currentUserId)
    │
    └─→ Response: Bookings for that specific user
```

## Seed Data Test Accounts

| Endpoint | Test Email | Test Password | User ID | Purpose |
|----------|-----------|---------------|---------|---------|
| POST /login | testlogin@example.com | SecurePassword123! | aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa | Full auth flow test |
| POST /verify-email | testverify@example.com | SecurePassword123! | aaaaaaaa-2222-2222-2222-aaaaaaaaaaaa | Email verification test |
| GET /current-user | testcurrentuser@example.com | SecurePassword123! | aaaaaaaa-3333-3333-3333-aaaaaaaaaaaa | Authenticated request test |

## Key Points

1. **Token is Stateless**: The token contains all user information, no server-side session needed
2. **Token in Every Request**: Must include token in Authorization header for ALL protected endpoints
3. **ICurrentUserService is the Bridge**: Connects token claims to your business logic
4. **Automatic User Association**: No need to pass userId in request body, it's extracted from token
5. **Role-Based Access**: Token contains role, used for authorization on protected endpoints

## Example: Adding Authentication to New Controller

```csharp
[ApiController]
[Route("api/listings")]
public class ListingsController : ControllerBase
{
    private readonly IListingService _listingService;
    private readonly ICurrentUserService _currentUserService;

    public ListingsController(IListingService listingService, 
                             ICurrentUserService currentUserService)
    {
        _listingService = listingService;
        _currentUserService = currentUserService;
    }

    // ✅ CORRECT: Uses ICurrentUserService to get authenticated user
    [HttpGet("my-listings")]
    public async Task<IActionResult> GetMyListings()
    {
        var currentUserId = _currentUserService.UserId;
        var listings = await _listingService.GetListingsByVendorAsync(currentUserId);
        return Ok(listings);
    }

    // ✅ CORRECT: Creates listing for current authenticated user
    [HttpPost("create")]
    public async Task<IActionResult> CreateListing(CreateListingRequest request)
    {
        var vendorId = _currentUserService.UserId;
        var listing = await _listingService.CreateListingAsync(request, vendorId);
        return Ok(listing);
    }
}
```

## Complete End-to-End Testing Workflow

### COMPLETE REGISTRATION + EMAIL VERIFICATION FLOW

#### Step 1: Register New User (Get Both JWT and Verification Token)
```bash
curl -X POST http://localhost:5037/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@example.com",
    "password": "SecurePassword123!",
    "firstName": "John",
    "lastName": "Doe"
  }'

# RESPONSE:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "userId": "f6ed1db7-1987-441f-84e6-4a1da5f4ba3f",
#   "email": "newuser@example.com",
#   "role": "User",
#   "verificationToken": "af1e7ce4-93d3-4fa0-bcb8-1f3ea09bcb01"  ← USE THIS TO VERIFY
# }
```

⚠️ **SAVE BOTH:**
- `token` → Use for authentication (Authorization header)
- `verificationToken` → Use to verify email (query parameter)

#### Step 2: Verify Email Using Verification Token (NOT the JWT)
```bash
# ✅ CORRECT: Use verificationToken from register response
curl -X POST 'http://localhost:5037/api/auth/verify-email?token=af1e7ce4-93d3-4fa0-bcb8-1f3ea09bcb01'

# RESPONSE:
# {
#   "message": "Email verified successfully"
# }

# ❌ WRONG: Don't use the JWT token
# This will return 404 "Invalid token"
# curl -X POST 'http://localhost:5037/api/auth/verify-email?token=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'
```

#### Step 3: Use JWT Token for Future Requests
```bash
curl -X GET http://localhost:5037/api/auth/current-user \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# RESPONSE:
# {
#   "userId": "f6ed1db7-1987-441f-84e6-4a1da5f4ba3f"
# }
```

---

## Testing Workflow (Using Seed Data)

### 1. Test Login
```bash
curl -X POST http://localhost:5037/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testlogin@example.com",
    "password": "SecurePassword123!"
  }'

# Response:
# {
#   "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
#   "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa"
# }
```

### 2. Save Token (in Postman or client app)
```
Set Authorization header for next request:
"Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### 3. Test Current User
```bash
curl -X GET http://localhost:5037/api/auth/current-user \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Response: { "userId": "aaaaaaaa-1111-1111-1111-aaaaaaaaaaaa" }
```

### 4. Test Other Endpoints with Same Token
```bash
curl -X GET http://localhost:5037/api/bookings/my-bookings \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Response: All bookings for that user (currentUserId automatically applied)
```

## Connection Summary

```
┌──────────────────┐
│   Login Request  │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐      ┌────────────────────┐
│  JWT Token       │─────→│ Authorization      │
│  (contains       │      │ Header in All      │
│   userId, email) │      │ Protected Requests │
└──────────────────┘      └────────┬───────────┘
                                   │
                                   ▼
                          ┌────────────────────┐
                          │ Middleware Extracts│
                          │ UserId from Claims │
                          └────────┬───────────┘
                                   │
                                   ▼
                          ┌────────────────────┐
                          │ ICurrentUserService│
                          │ Provides UserId    │
                          └────────┬───────────┘
                                   │
                                   ▼
                          ┌────────────────────┐
                          │ All Endpoints Know │
                          │ Current User       │
                          │ Automatically      │
                          └────────────────────┘
```
