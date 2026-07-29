using Vault.Core;

namespace Vault.Facade.Interface;

public interface IVaultFacade
{
    Task AddEntryAsync(
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        string tags,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default);
}