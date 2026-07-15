namespace Vault.Core.Abstractions;

public interface IVaultFileStore
{
    bool Exists();

    void Save(byte[] nonce, byte[] tag, byte[] cipherText);
    (byte[] nonce, byte[] tag, byte[] cipherText) Load();
}
    