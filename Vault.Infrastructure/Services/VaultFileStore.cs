using System.Text;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultFileStore : IVaultFileStore
{
    // Header: PCV1(4) + version(1) + salt(16) + 4 ints(16) = 37 bytes
    // Body start: nonce(12) + tag(16) + length(4) = 32 bytes
    private const int HeaderSize = 4 + 1 + 16 + (4 * 4);
    private const int MinBodyPrefixSize = 12 + 16 + 4;

    public bool Exists() => File.Exists(GetVaultPath());

    public void Save(byte[] nonce, byte[] tag, byte[] cipherText, KdfParams kdf)
    {
        string vaultPath = GetVaultPath();
        string? directory = Path.GetDirectoryName(vaultPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(vaultPath);
        using BinaryWriter writer = new(stream);

        WriteHeader(writer, stream, kdf);

        stream.Write(nonce);
        stream.Write(tag);
        writer.Write(cipherText.Length);
        stream.Write(cipherText);
    }

    public KdfParams ReadHeader()
    {
        using FileStream stream = File.OpenRead(GetVaultPath());
        if (stream.Length < HeaderSize)
        {
            throw new InvalidDataException("Vault file is too small to contain a valid header.");
        }

        using BinaryReader reader = new(stream);
        return ReadHeader(reader);
    }

    public (byte[] nonce, byte[] tag, byte[] cipherText, KdfParams kdf) Load()
    {
        using FileStream stream = File.OpenRead(GetVaultPath());
        if (stream.Length < HeaderSize + MinBodyPrefixSize)
        {
            throw new InvalidDataException("Vault file is too small to contain valid data.");
        }

        using BinaryReader reader = new(stream);

        KdfParams kdf = ReadHeader(reader);

        byte[] nonce = reader.ReadBytes(12);
        byte[] tag = reader.ReadBytes(16);
        if (nonce.Length != 12 || tag.Length != 16)
        {
            throw new InvalidDataException("Vault file is corrupted or has an invalid format.");
        }

        int cipherLength = reader.ReadInt32();
        if (cipherLength < 0 || cipherLength != stream.Length - stream.Position)
        {
            throw new InvalidDataException("Vault file has an invalid ciphertext length.");
        }

        byte[] cipherText = reader.ReadBytes(cipherLength);
        if (cipherText.Length != cipherLength)
        {
            throw new InvalidDataException("Vault file is corrupted or has an invalid format.");
        }

        return (nonce, tag, cipherText, kdf);
    }

    public string GetVaultPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PassCrypt",
            "vault.bin");
    }

    private static void WriteHeader(BinaryWriter writer, FileStream stream, KdfParams kdf)
    {
        stream.Write(Encoding.ASCII.GetBytes("PCV1"));
        writer.Write((byte)1);
        stream.Write(kdf.Salt);
        writer.Write(kdf.MemoryKib);
        writer.Write(kdf.Iterations);
        writer.Write(kdf.Parallelism);
        writer.Write(kdf.KeyLength);
    }

    private static KdfParams ReadHeader(BinaryReader reader)
    {
        byte[] magic = reader.ReadBytes(4);
        if (magic.Length != 4 || Encoding.ASCII.GetString(magic) != "PCV1")
        {
            throw new InvalidDataException(
                "Vault file is missing the PCV1 header. Delete the old vault.bin/salt.bin and unlock again to create a new vault.");
        }

        byte version = reader.ReadByte();
        if (version != 1)
        {
            throw new InvalidDataException($"Unsupported vault header version: {version}.");
        }

        byte[] salt = reader.ReadBytes(16);
        if (salt.Length != 16)
        {
            throw new InvalidDataException("Vault header has an invalid salt.");
        }

        int memoryKib = reader.ReadInt32();
        int iterations = reader.ReadInt32();
        int parallelism = reader.ReadInt32();
        int keyLength = reader.ReadInt32();

        return new KdfParams(salt, memoryKib, iterations, parallelism, keyLength);
    }
}
