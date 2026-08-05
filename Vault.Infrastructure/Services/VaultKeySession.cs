using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* Ows the session's cryptographic key lifecycle only Derive() loads/creates the salt, derives the AES key via KeyDerivation */
public class VaultKeySession : IVaultKeySession
{
    private readonly ILoadCreateSalt _loadCreateSalt;
    private readonly IKeyDerivation _keyDerivation;

    private byte[]? _sessionKey;

    public VaultKeySession(ILoadCreateSalt loadCreateSalt, IKeyDerivation keyDerivation)
    {
        _loadCreateSalt = loadCreateSalt;
        _keyDerivation = keyDerivation;
    }

    public bool IsUnlocked => _sessionKey is not null;

    // Derives the session key from the master password and clears the
    // password bytes immediately afterward, whether derivation succeeded or not.
    public void Derive(byte[] masterPassword)
    {
        try
        {
            byte[] salt = _loadCreateSalt.LoadOrCreateSalt();
            _sessionKey = _keyDerivation.DeriveKey(masterPassword, salt);
        }
        finally
        {
            Array.Clear(masterPassword, 0, masterPassword.Length);
        }
    }

    public byte[] RequireSessionKey()
    {
        if (_sessionKey is null)
            throw new UnauthorizedAccessException("Vault is locked.");

        return _sessionKey;
    }

    public void Clear()
    {
        if (_sessionKey is not null)
        {
            Array.Clear(_sessionKey, 0, _sessionKey.Length);
            _sessionKey = null;
        }
    }
}
