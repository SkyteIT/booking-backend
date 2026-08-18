using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Listing> Listings { get; }
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
