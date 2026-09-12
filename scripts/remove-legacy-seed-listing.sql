-- Verified against the original TestDataSeeder in commit efc607b.
-- Only the historical seed listing and its two seed bookings are eligible.
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;
DECLARE @SeedListing uniqueidentifier = 'dddddddd-dddd-dddd-dddd-dddddddddddd';
SELECT * INTO #PreservedListings FROM dbo.Listings WITH (UPDLOCK, HOLDLOCK)
WHERE Id <> @SeedListing;
IF EXISTS (SELECT 1 FROM dbo.Listings WHERE Id = @SeedListing AND
    (VendorProfileId <> 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
     OR Title <> 'Event Photography - Basic'
     OR Description <> 'Seed listing for booking status transition testing.'))
    THROW 50001, 'Seed listing identity does not match; nothing deleted.', 1;
IF EXISTS (SELECT 1 FROM dbo.Bookings WHERE ListingId = @SeedListing AND Id NOT IN
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'ffffffff-ffff-ffff-ffff-ffffffffffff'))
    THROW 50002, 'Non-seed bookings exist; nothing deleted.', 1;
IF EXISTS (SELECT 1 FROM dbo.Reviews WHERE ListingId = @SeedListing OR BookingId IN
    (SELECT Id FROM dbo.Bookings WHERE ListingId = @SeedListing))
    THROW 50003, 'Reviews exist; nothing deleted.', 1;
DELETE FROM dbo.Bookings WHERE ListingId = @SeedListing AND Id IN
    ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', 'ffffffff-ffff-ffff-ffff-ffffffffffff');
DECLARE @BookingsDeleted int = @@ROWCOUNT;
DELETE FROM dbo.Listings WHERE Id = @SeedListing;
DECLARE @ListingsDeleted int = @@ROWCOUNT;
IF EXISTS (SELECT * FROM #PreservedListings EXCEPT SELECT * FROM dbo.Listings)
    OR EXISTS (SELECT * FROM dbo.Listings EXCEPT SELECT * FROM #PreservedListings)
    THROW 50004, 'Other listings changed; rolling back.', 1;
COMMIT TRANSACTION;
SELECT @ListingsDeleted AS SeedListingsDeleted, @BookingsDeleted AS SeedBookingsDeleted,
    (SELECT COUNT(*) FROM #PreservedListings) AS OtherListingsPreserved,
    (SELECT COUNT(*) FROM dbo.Listings) AS RemainingListings;
