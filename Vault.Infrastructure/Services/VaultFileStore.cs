using System.Security.Cryptography;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultFileStore : IVaultFileStore
{
    public bool Exists() => File.Exists(GetVaultPath());

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

        stream.Write(nonce);
        stream.Write(tag);
        writer.Write(cipherText.Length);
        stream.Write(cipherText);
    }

    public (byte[] nonce, byte[] tag, byte[] cipherText) Load()
    {
        using FileStream stream = File.OpenRead(GetVaultPath());
        using BinaryReader reader = new(stream);

        byte[] nonce = reader.ReadBytes(AesGcm.NonceByteSizes.MaxSize);
        byte[] tag = reader.ReadBytes(AesGcm.TagByteSizes.MaxSize);
        int cipherLength = reader.ReadInt32();
        byte[] cipherText = reader.ReadBytes(cipherLength);

        return (nonce, tag, cipherText);
    }

    public string GetVaultPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PassCrypt",
            "vault.bin");
    }
}
