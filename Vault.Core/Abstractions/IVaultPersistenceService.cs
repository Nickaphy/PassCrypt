namespace Vault.Core.Abstractions;

public interface IVaultPersistenceService
{
    bool VaultExists();

    void Save(byte[] sessionKey, IReadOnlyList<VaultEntry> entries);
    List<VaultEntry> Load(byte[] sessionKey);
}
