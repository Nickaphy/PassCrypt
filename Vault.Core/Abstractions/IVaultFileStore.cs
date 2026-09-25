using Vault.Core;

namespace Vault.Core.Abstractions;

public interface IVaultFileStore
{
    bool Exists();

    KdfParams ReadHeader();

    void Save(byte[] nonce, byte[] tag, byte[] cipherText, KdfParams kdf);

    (byte[] nonce, byte[] tag, byte[] cipherText, KdfParams kdf) Load();
}
