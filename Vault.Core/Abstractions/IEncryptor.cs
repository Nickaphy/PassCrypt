namespace Vault.Core.Abstractions;

public interface IEncryptor
{
    byte[] Encrypt(byte[] key, byte[] plainText, out byte[] nonce, out byte[] tag);
}
