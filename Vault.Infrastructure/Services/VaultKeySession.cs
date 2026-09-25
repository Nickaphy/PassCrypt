using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

/* Owns the session's cryptographic key lifecycle. Derive() takes KdfParams
   (salt + Argon2 settings from the vault header) and holds both the AES key
   and those params for later Save calls. */
public class VaultKeySession : IVaultKeySession
{
    private readonly IKeyDerivation _keyDerivation;

    private byte[]? _sessionKey;
    private KdfParams? _kdfParams;

    public VaultKeySession(IKeyDerivation keyDerivation)
    {
        _keyDerivation = keyDerivation;
    }

    public bool IsUnlocked => _sessionKey is not null;

    public void Derive(byte[] masterPassword, KdfParams kdf)
    {
        try
        {
            _sessionKey = _keyDerivation.DeriveKey(masterPassword, kdf);
            _kdfParams = kdf;
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

    public KdfParams RequireKdfParams()
    {
        if (_kdfParams is null)
            throw new UnauthorizedAccessException("Vault is locked.");

        return _kdfParams;
    }

    public void Clear()
    {
        if (_sessionKey is not null)
        {
            Array.Clear(_sessionKey, 0, _sessionKey.Length);
            _sessionKey = null;
        }

        _kdfParams = null;
    }
}
