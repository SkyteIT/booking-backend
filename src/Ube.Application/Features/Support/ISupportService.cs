namespace Ube.Application.Features.Support;

public interface ISupportService
{
    Task SubmitTicketAsync(Guid userId, SubmitSupportTicketDto dto, CancellationToken ct = default);
}
