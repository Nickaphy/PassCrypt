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

    /*
    opens `vault.bin`
    2. reads the stored `nonce`
    3. reads the stored `tag`
    4. reads the ciphertext length
    5. reads the ciphertext bytes
    6. returns all three so the decryptor can turn them back into plaintext entries
    */
    public (byte[] nonce, byte[] tag, byte[] cipherText) Load()
    {
        // Open the vault file for reading.
        using FileStream stream = File.OpenRead(GetVaultPath());
        // Validate that the file is large enough to contain the nonce, tag, and length of the ciphertext.
        if (stream.Length < 12 + 16 + 4)
        {
            throw new InvalidDataException("Vault file is too small to contain valid data.");
        }

        using BinaryReader reader = new(stream);

        // Read the same layout that Save() writes.
        byte[] nonce = reader.ReadBytes(12); // AES-GCM nonce is 12 bytes.

        // Validate the nonce length.
        byte[] tag = reader.ReadBytes(16); // AES-GCM tag is 16 bytes.
        if (nonce.Length != 12 || tag.Length != 16)
        {
            throw new InvalidDataException("Vault file is corrupted or has an invalid format.");
        }

        // Read the length of the ciphertext and validate it.
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
