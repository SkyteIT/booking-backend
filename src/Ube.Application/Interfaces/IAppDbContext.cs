using Microsoft.EntityFrameworkCore;
using Ube.Domain.Entities.Listings;

namespace Ube.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<Category> Categories { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}