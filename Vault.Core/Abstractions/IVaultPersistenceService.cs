using Vault.Core;

namespace Vault.Core.Abstractions;

public interface IVaultPersistenceService
{
    bool VaultExists();

    KdfParams ReadHeader();

    void Save(byte[] sessionKey, KdfParams kdf, IReadOnlyList<VaultEntry> entries);

    List<VaultEntry> Load(byte[] sessionKey);
}
