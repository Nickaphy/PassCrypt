using System.Security.Cryptography;

namespace Vault.Infrastructure;

// Encrypts plain bytes with AES-GCM.
public class Encryptor
{
    public byte[] Encrypt(byte[] key, byte[] plainText, out byte[] nonce, out byte[] tag)
    {
        nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes
        byte[] cipherText = new byte[plainText.Length];

        using var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainText, cipherText, tag);

        return cipherText;
    }
}
