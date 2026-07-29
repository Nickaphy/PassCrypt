using Vault.Application.Commands;
using Vault.Application.Interface;
using Vault.Core;
using Vault.Facade.Interface;

namespace Vault.Facade.Services;

public class VaultFacade : IVaultFacade
{
    private readonly IEntryApplicationService _entryApplicationService;

    public VaultFacade(IEntryApplicationService entryApplicationService)
    {
        _entryApplicationService = entryApplicationService;
    }

    public Task AddEntryAsync(
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        string tags,
        CancellationToken cancellationToken = default)
    {
        // Packs the raw UI strings into CreateEntryCommand
        var command = new CreateEntryCommand(
            entryName,
            category,
            url,
            username,
            password,
            notes,
            tags);

        //calling AddEntryAsync in Application
        return _entryApplicationService.AddEntryAsync(command, cancellationToken);
    }

    public Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        return _entryApplicationService.GetEntriesAsync(cancellationToken);
    }
}
