using Vault.Core;
using Vault.Core.Abstractions;
using Vault.Facade.Interface;

namespace Vault.Infrastructure.Services;

// View entry CQRS
public class VaultEntryQueryService : IVaultEntryQueryService
{
    private readonly IVaultSessionService _vaultSessionService;

    public VaultEntryQueryService(IVaultSessionService vaultSessionService)
    {
        _vaultSessionService = vaultSessionService;
    }

    public Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_vaultSessionService.GetEntries());
    }
}