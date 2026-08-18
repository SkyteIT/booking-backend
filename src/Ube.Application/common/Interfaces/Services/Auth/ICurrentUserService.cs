

namespace Ube.Application.Common.Interfaces.Services.Auth;

public interface ICurrentUserService
{
    Guid UserId { get; }
}