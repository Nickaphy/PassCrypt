using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* Thin coordinator implementing IVaultSessionService, the only thing the rest of the app (UI,
  EntryApplicationService) talks to. Every method follows the same shape: guard via VaultKeySession, delegate to
  VaultEntryRepository, persist via VaultPersistenceService. No logic of its own beyond sequencing. */

public class VaultSessionService : IVaultSessionService
{
    private readonly IVaultKeySession _keySession;
    private readonly IVaultEntryRepository _entryRepository;
    private readonly IVaultPersistenceService _persistenceService;

    public VaultSessionService(
        IVaultKeySession keySession,
        IVaultEntryRepository entryRepository,
        IVaultPersistenceService persistenceService)
    {
        _keySession = keySession;
        _entryRepository = entryRepository;
        _persistenceService = persistenceService;
    }

    public Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _keySession.Derive(masterPassword);
        byte[] sessionKey = _keySession.RequireSessionKey();

        if (!_persistenceService.VaultExists())
        {
            _entryRepository.Clear();
            _persistenceService.Save(sessionKey, _entryRepository.GetAll());
        }
        else
        {
            List<VaultEntry> entries = _persistenceService.Load(sessionKey);
            _entryRepository.ReplaceAll(entries);
        }

        return Task.CompletedTask;
    }

    public void AddEntry(VaultEntry entry)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Add(entry);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public void DeleteEntry(Guid entryId)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Delete(entryId);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public void UpdateEntry(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Update(entryId, entryName, category, url, username, password, notes, tags);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public IReadOnlyList<VaultEntry> GetEntries()
    {
        _keySession.RequireSessionKey();
        return _entryRepository.GetAll();
    }

    public void LockVault()
    {
        _keySession.Clear();
        _entryRepository.Clear();
    }
}
