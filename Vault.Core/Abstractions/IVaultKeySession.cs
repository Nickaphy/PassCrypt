namespace Vault.Core.Abstractions;

public interface IVaultKeySession
{
    bool IsUnlocked { get; }

    void Derive(byte[] masterPassword);
    byte[] RequireSessionKey();
    void Clear();
}
