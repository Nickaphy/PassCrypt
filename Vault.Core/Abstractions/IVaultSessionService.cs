namespace Vault.Core.Abstractions;

public interface IVaultSessionService
{
    Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default);
    void AddEntry(VaultEntry entry);
    IReadOnlyList<VaultEntry> GetEntries();
    void LockVault();
} 