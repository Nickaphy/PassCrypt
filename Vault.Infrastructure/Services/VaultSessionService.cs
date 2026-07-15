using System.Security.Cryptography;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultSessionService : IVaultSessionService
{
    //Dependency injection for the key derivation, encryptor and decryptor and load create salt.
    private readonly LoadCreateSalt _loadCreateSalt;
    private readonly KeyDerivation _keyDerivation;
    private readonly Decryptor _decryptor;
    private readonly Encryptor _encryptor;
    private readonly IVaultFileStore _vaultFileStore;

    public VaultSessionService(KeyDerivation keyDerivation, Decryptor decryptor, Encryptor encryptor, LoadCreateSalt loadCreateSalt, IVaultFileStore vaultFileStore)
    {
        _keyDerivation = keyDerivation;
        _decryptor = decryptor;
        _encryptor = encryptor;
        _loadCreateSalt = loadCreateSalt;
        _vaultFileStore = vaultFileStore;
    }

    //Method resonsible for unlocking the vault with the derived key.
    public Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Nullable container for the derived key.
        byte[]? derivedKey = null;

        try
        {
            // Load or create the salt 
            byte[] salt = _loadCreateSalt.LoadOrCreateSalt();
            // Derive the key from the master password and the salt.
            derivedKey = _keyDerivation.DeriveKey(masterPassword, salt);

            if (!_vaultFileStore.Exists())
            {
                byte[] verification = System.Text.Encoding.UTF8.GetBytes("PassCrypt-Vault");

                byte[] cipherText = _encryptor.Encrypt(
                    derivedKey,
                    verification,
                    out byte[] nonce,
                    out byte[] tag);
                
                _vaultFileStore.Save(nonce, tag, cipherText);
            } 
            // If the vault file exists, load the nonce, tag and cipher text.
            else {
                var (nonce, tag, cipherText) = _vaultFileStore.Load();

                // Decrypt the vault with the derived key.
                _decryptor.Decrypt(derivedKey, nonce, tag, cipherText);
            }
            return Task.CompletedTask;
        }
        finally
        {
            // Clear the derived key from memory.
            if (derivedKey is not null)
            {
                Array.Clear(derivedKey, 0, derivedKey.Length);
            }

            Array.Clear(masterPassword, 0, masterPassword.Length);
        }
    }
}
