using Vault.Application.Commands;
using Vault.Core;

namespace Vault.Application.Interface;

public interface IEntryApplicationService
{
    Task AddEntryAsync(CreateEntryCommand command, CancellationToken cancellationToken = default);
    
    Task UpdateEntryAsync(UpdateEntryCommand command, CancellationToken cancellationToken = default);
    
    Task DeleteEntryAsync(Guid entryId, CancellationToken cancellationToken = default);
}


