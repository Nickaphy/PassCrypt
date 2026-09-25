using Vault.Core;

namespace Vault.Core.Abstractions;

public interface IVaultKeySession
{
    bool IsUnlocked { get; }

    void Derive(byte[] masterPassword, KdfParams kdf);
    byte[] RequireSessionKey();
    KdfParams RequireKdfParams();
    void Clear();
}
