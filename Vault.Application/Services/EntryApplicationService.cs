using Vault.Application.Commands;
using Vault.Application.Interface;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Application.Services;

public class EntryApplicationService : IEntryApplicationService
{
    private readonly IVaultSessionService _vaultSessionService;

    public EntryApplicationService(IVaultSessionService vaultSessionService)
    {
        _vaultSessionService = vaultSessionService;
    }

    public Task AddEntryAsync(CreateEntryCommand command, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Splitting the tags by commma
        string[] parsedTags = command.Tags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        // Converting command data to core objects
        var newEntry = new VaultEntry(
            command.EntryName,
            command.Category,
            command.Url,
            command.Username,
            command.Password,
            command.Notes,
            parsedTags);

        _vaultSessionService.AddEntry(newEntry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<VaultEntry>> GetEntriesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<VaultEntry> entries = _vaultSessionService.GetEntries();
        return Task.FromResult(entries);
    }
}