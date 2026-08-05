using System.Text;
using System.Text.Json;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* The translation layer between in-memory entries and the encrypted file on disk. Save serializes →
  encrypts (via Encryptor) → hands bytes to IVaultFileStore. Load does the reverse via Decryptor. VaultExists() passes through to
  the file store so nothing upstream needs to know IVaultFileStore exists. */

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

    public void Save(byte[] sessionKey, IReadOnlyList<VaultEntry> entries)
    {
        string json = JsonSerializer.Serialize(entries);
        byte[] plainText = Encoding.UTF8.GetBytes(json);

        byte[] cipherText = _encryptor.Encrypt(sessionKey, plainText, out byte[] nonce, out byte[] tag);

        _vaultFileStore.Save(nonce, tag, cipherText);
    }

    public List<VaultEntry> Load(byte[] sessionKey)
    {
        var (nonce, tag, cipherText) = _vaultFileStore.Load();

        byte[] decryptedBytes = _decryptor.Decrypt(sessionKey, nonce, tag, cipherText);
        string json = Encoding.UTF8.GetString(decryptedBytes);

        return JsonSerializer.Deserialize<List<VaultEntry>>(json) ?? new List<VaultEntry>();
    }
}
