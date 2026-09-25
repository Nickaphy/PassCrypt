using System.Text;
using System.Text.Json;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* Translation layer between in-memory entries and the encrypted file on disk.
   Save serializes → encrypts → writes header + body via IVaultFileStore.
   ReadHeader exposes plaintext KDF params before a session key exists. */
public class VaultPersistenceService : IVaultPersistenceService
{
    private readonly IEncryptor _encryptor;
    private readonly IDecryptor _decryptor;
    private readonly IVaultFileStore _vaultFileStore;

    public VaultPersistenceService(IEncryptor encryptor, IDecryptor decryptor, IVaultFileStore vaultFileStore)
    {
        _encryptor = encryptor;
        _decryptor = decryptor;
        _vaultFileStore = vaultFileStore;
    }

    public bool VaultExists() => _vaultFileStore.Exists();

    public KdfParams ReadHeader() => _vaultFileStore.ReadHeader();

    public void Save(byte[] sessionKey, KdfParams kdf, IReadOnlyList<VaultEntry> entries)
    {
        string json = JsonSerializer.Serialize(entries);
        byte[] plainText = Encoding.UTF8.GetBytes(json);

        byte[] cipherText = _encryptor.Encrypt(sessionKey, plainText, out byte[] nonce, out byte[] tag);

        _vaultFileStore.Save(nonce, tag, cipherText, kdf);
    }

    public List<VaultEntry> Load(byte[] sessionKey)
    {
        var (nonce, tag, cipherText, _) = _vaultFileStore.Load();

        byte[] decryptedBytes = _decryptor.Decrypt(sessionKey, nonce, tag, cipherText);
        string json = Encoding.UTF8.GetString(decryptedBytes);

        return JsonSerializer.Deserialize<List<VaultEntry>>(json) ?? new List<VaultEntry>();
    }
}
