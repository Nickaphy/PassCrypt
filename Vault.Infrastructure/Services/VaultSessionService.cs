using System.Security.Cryptography;
using System.Text.Json;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultSessionService : IVaultSessionService
{
    private readonly LoadCreateSalt _loadCreateSalt;
    private readonly KeyDerivation _keyDerivation;
    private readonly Decryptor _decryptor;
    private readonly Encryptor _encryptor;
    private readonly IVaultFileStore _vaultFileStore;

    // Session state
    private byte[]? _sessionKey;
    private List<VaultEntry> _entries = new();

    public VaultSessionService(KeyDerivation keyDerivation, Decryptor decryptor, Encryptor encryptor, LoadCreateSalt loadCreateSalt, IVaultFileStore vaultFileStore)
    {
        _keyDerivation = keyDerivation;
        _decryptor = decryptor;
        _encryptor = encryptor;
        _loadCreateSalt = loadCreateSalt;
        _vaultFileStore = vaultFileStore;
    }

    public Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            byte[] salt = _loadCreateSalt.LoadOrCreateSalt();
            
            // Store the key in memory instead of a local variable
            _sessionKey = _keyDerivation.DeriveKey(masterPassword, salt);

            if (!_vaultFileStore.Exists())
            {
                _entries = new List<VaultEntry>();
                SaveVault(); // Creates the initial encrypted JSON file
            } 
            else 
            {
                var (nonce, tag, cipherText) = _vaultFileStore.Load();

                byte[] decryptedBytes = _decryptor.Decrypt(_sessionKey, nonce, tag, cipherText);
                string json = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                
                // Deserialize the payload back into the domain objects
                _entries = JsonSerializer.Deserialize<List<VaultEntry>>(json) ?? new List<VaultEntry>();
            }
            return Task.CompletedTask;
        }
        finally
        {
            // Only clear the master password here. 
            // The session key must remain active until LockVault() is called.
            Array.Clear(masterPassword, 0, masterPassword.Length);
        }
    }

    // Puts the entry in the in-memory list and calls SaveVault()
    public void AddEntry(VaultEntry entry)
    {
        if (_sessionKey == null) throw new UnauthorizedAccessException("Vault is locked.");
        ArgumentNullException.ThrowIfNull(entry);

        _entries.Add(entry);
        SaveVault();
    }

    // Returns a copy of the in-memory list of entries
    public IReadOnlyList<VaultEntry> GetEntries()
    {
        if (_sessionKey == null) throw new UnauthorizedAccessException("Vault is locked.");
        
        return _entries.ToList();
    }

    public void LockVault()
    {
        if (_sessionKey is not null)
        {
            Array.Clear(_sessionKey, 0, _sessionKey.Length);
            _sessionKey = null;
        }
        _entries.Clear();
    }

    private void SaveVault()
    {
        if (_sessionKey == null)
        {
            throw new UnauthorizedAccessException("Vault is locked.");
        }

        // Convert the list to JSON string and then to bytes
        string json = JsonSerializer.Serialize(_entries);
        byte[] plainText = System.Text.Encoding.UTF8.GetBytes(json);

        byte[] cipherText = _encryptor.Encrypt(
            _sessionKey, 
            plainText, 
            out byte[] nonce, 
            out byte[] tag);

        _vaultFileStore.Save(nonce, tag, cipherText);
    }
}

