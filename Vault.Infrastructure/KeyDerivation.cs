using Konscious.Security.Cryptography;

namespace Vault.Infrastructure;

// File responsible for turning master key into a key for AES encryption.
public class KeyDerivation
{
    // Key derivation (Argon2id)
    public byte[] DeriveKey(byte[] password, byte[] salt)
    {
        using var argon2 = new Argon2id(password);
        argon2.Salt = salt; // The salt is used to make the key unique.
        argon2.DegreeOfParallelism = 8; // The number of threads to use.
        argon2.Iterations = 4; // The number of iterations to perform.
        argon2.MemorySize = 1024 * 64; // The memory size to use.
        return argon2.GetBytes(32); // The key is 32 bytes long.
    }
}
