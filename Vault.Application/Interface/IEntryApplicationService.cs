using Vault.Application.Commands;
using Vault.Core;

namespace Vault.Application.Interface;

public interface IEntryApplicationService
{
    Task AddEntryAsync(CreateEntryCommand command, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);
}