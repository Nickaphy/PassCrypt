using Vault.Application.Commands;
using Vault.Application.Interface;
using Vault.Core;
using Vault.Core.Abstractions;
using Vault.Facade.Interface;

namespace Vault.Facade.Services;

public class VaultFacade : IVaultFacade
{
    private readonly IEntryApplicationService _entryApplicationService;
    private readonly IVaultEntryQueryService _vaultEntryQueryService;

    public VaultFacade(IVaultEntryQueryService vaultEntryQueryService, IEntryApplicationService entryApplicationService)
    {
        _vaultEntryQueryService = vaultEntryQueryService;
        _entryApplicationService = entryApplicationService;
    }

    //Interface Implementation
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

    public Task UpdateEntryAsync(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        string tags,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateEntryCommand(
            entryId,
            entryName,
            category,
            url,
            username,
            password,
            notes,
            tags);
           
        return _entryApplicationService.UpdateEntryAsync(command, cancellationToken);
    }
    

    public Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        return _vaultEntryQueryService.GetEntriesAsync(cancellationToken);
    }
}
