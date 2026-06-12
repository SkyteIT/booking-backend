namespace Ube.Application.Features.Security;

public interface ISecurityService
{
    Task ChangePasswordAsync(Guid userId, ChangePasswordDto dto);
}
