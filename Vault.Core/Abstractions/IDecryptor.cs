namespace Vault.Core.Abstractions;

public interface IDecryptor
{
    byte[] Decrypt(byte[] key, byte[] nonce, byte[] tag, byte[] cipherText);
}
