using System.Security.Cryptography;

namespace Vault.Core;
// Change from hardcoded argon parameters to storing params for vault in vault.bin header
public sealed class KdfParams
{
    public byte[] Salt { get; }
    public int MemoryKib { get; }
    public int Iterations { get; }
    public int Parallelism { get; }
    public int KeyLength { get; }

    public KdfParams(byte[] salt, int memoryKib, int iterations, int parallelism, int keyLength)
    {
        Salt = salt;
        MemoryKib = memoryKib;
        Iterations = iterations;
        Parallelism = parallelism;
        KeyLength = keyLength;
    }

    public static KdfParams CreateDefault()
    {
        return new KdfParams(
            salt: RandomNumberGenerator.GetBytes(16),
            memoryKib: 1024 * 64,
            iterations: 4,
            parallelism: 8,
            keyLength: 32
        );
    }
}        
        