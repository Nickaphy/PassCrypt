namespace Vault.Core.Abstractions;

public interface IVaultEntryRepository
{
    void ReplaceAll(IEnumerable<VaultEntry> entries);
    IReadOnlyList<VaultEntry> GetAll();

    void Add(VaultEntry entry);
    void Delete(Guid entryId);

    void Update(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags);

    void Clear();
}
