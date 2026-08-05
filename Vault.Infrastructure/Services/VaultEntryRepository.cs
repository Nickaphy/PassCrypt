using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* In-memory CRUD over List<VaultEntry>. Add/Delete/Update/GetAll/Clear/ReplaceAll (used once, right
  after unlock, to load persisted entries in). FindOrThrow deduplicates the "entry not found" check used by Delete and Update.
  GetAll() returns a copy, so callers can't mutate internal state through the reference. Knows nothing about crypto or files. */

public class VaultEntryRepository : IVaultEntryRepository
{
    private List<VaultEntry> _entries = new();

    public void ReplaceAll(IEnumerable<VaultEntry> entries)
    {
        _entries = entries.ToList();
    }

    public IReadOnlyList<VaultEntry> GetAll() => _entries.ToList();

    public void Add(VaultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
    }

    public void Delete(Guid entryId)
    {
        VaultEntry entry = FindOrThrow(entryId);
        _entries.Remove(entry);
    }

    public void Update(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        VaultEntry entry = FindOrThrow(entryId);
        entry.UpdateDetails(entryName, category, url, username, password, notes, tags);
    }

    public void Clear() => _entries.Clear();

    private VaultEntry FindOrThrow(Guid entryId)
    {
        VaultEntry? entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null)
            throw new InvalidOperationException($"Entry with ID '{entryId}' was not found.");

        return entry;
    }
}
