using Konscious.Security.Cryptography;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure;

// Derives the AES key from the master password using params from the vault header.
public class KeyDerivation : IKeyDerivation
{
    public byte[] DeriveKey(byte[] password, KdfParams kdf)
    {
        using var argon2 = new Argon2id(password);
        argon2.Salt = kdf.Salt;
        argon2.DegreeOfParallelism = kdf.Parallelism;
        argon2.Iterations = kdf.Iterations;
        argon2.MemorySize = kdf.MemoryKib;
        return argon2.GetBytes(kdf.KeyLength);
    }
}
