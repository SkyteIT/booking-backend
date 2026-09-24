using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Listings;
using Ube.Domain.Entities.Users;
using Ube.Domain.Entities.Vendors;

namespace Ube.Application.Common.Interfaces.Persistence;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<Listing> Listings { get; }
    DbSet<Category> Categories { get; }
    DbSet<VendorCategoryRequest> VendorCategoryRequests { get; }
    DbSet<VendorApplication> VendorApplications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


