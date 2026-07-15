using System.Security.Cryptography;

namespace Vault.Infrastructure;

// Reverses encryption and turns ciphertext back into readable text.
public class Decryptor
{
    public byte[] Decrypt(byte[] key, byte[] nonce, byte[] tag, byte[] cipherText)
    {
        byte[] plainText = new byte[cipherText.Length];

        using var aesGcm = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Decrypt(nonce, cipherText, tag, plainText);

        return plainText;
    }
}
