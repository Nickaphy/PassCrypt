using Vault.Core;

namespace Vault.Facade.Interface;

public interface IVaultEntryQueryService
{
    Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);
}
