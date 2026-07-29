using System.Security.Cryptography;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultFileStore : IVaultFileStore
{
    // Checks whether the vault file already exists.
    public bool Exists() => File.Exists(GetVaultPath());

    // Saves the encrypted vault bytes to disk.
    public void Save(byte[] nonce, byte[] tag, byte[] cipherText)
    {
        string vaultPath = GetVaultPath();
        string? directory = Path.GetDirectoryName(vaultPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using FileStream stream = File.Create(vaultPath);
        using BinaryWriter writer = new(stream);

        // File layout is: nonce, tag, length, ciphertext.
        stream.Write(nonce);
        stream.Write(tag);
        writer.Write(cipherText.Length);
        stream.Write(cipherText);
    }

    // Loads the encrypted vault bytes from disk.
    public (byte[] nonce, byte[] tag, byte[] cipherText) Load()
    {
        using FileStream stream = File.OpenRead(GetVaultPath());
        using BinaryReader reader = new(stream);

        // Read the same layout that Save() writes.
        byte[] nonce = reader.ReadBytes(AesGcm.NonceByteSizes.MaxSize);
        byte[] tag = reader.ReadBytes(AesGcm.TagByteSizes.MaxSize);
        int cipherLength = reader.ReadInt32();
        byte[] cipherText = reader.ReadBytes(cipherLength);

        return (nonce, tag, cipherText);
    }

    // Returns the vault file path under LocalApplicationData.
    public string GetVaultPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PassCrypt",
            "vault.bin");
    }
}
