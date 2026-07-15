using System.Security.Cryptography;
using System.IO;

namespace Vault.Infrastructure.Services;

public class LoadCreateSalt
{
    //Method responsible for loading or creating the salt.    
    public byte[] LoadOrCreateSalt()
    {
        string saltPath = GetSaltPath();
        string? directory = Path.GetDirectoryName(saltPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(saltPath))
        {
            return File.ReadAllBytes(saltPath);
        }

        byte[] salt = RandomNumberGenerator.GetBytes(16);
        File.WriteAllBytes(saltPath, salt);
        return salt;
    }

    public string GetSaltPath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PassCrypt",
                "salt.bin");
        
    }
}