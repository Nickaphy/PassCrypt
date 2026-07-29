using Konscious.Security.Cryptography;

namespace Vault.Infrastructure;

// Derives the AES key from the master password.
public class KeyDerivation
{
    // Use Argon2id to derive the key.
    public byte[] DeriveKey(byte[] password, byte[] salt)
    {
        using var argon2 = new Argon2id(password);
        argon2.Salt = salt; // Salt makes the key unique.
        argon2.DegreeOfParallelism = 8; // Number of threads.
        argon2.Iterations = 4; // Number of passes.
        argon2.MemorySize = 1024 * 64; // Memory cost.
        return argon2.GetBytes(32); // Return a 32-byte key.
    }
}
