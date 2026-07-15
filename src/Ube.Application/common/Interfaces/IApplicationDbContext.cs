using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Ube.Domain.Entities.Users;
using Ube.Application.Common.Interfaces;
namespace Ube.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}