namespace Vault.Core.Abstractions;

public interface IVaultSessionService
{
    Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default);
    void AddEntry(VaultEntry entry);
    IReadOnlyList<VaultEntry> GetEntries();


    void DeleteEntry(Guid entryId);
    
    void UpdateEntry(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags);
    
    void LockVault();
} 