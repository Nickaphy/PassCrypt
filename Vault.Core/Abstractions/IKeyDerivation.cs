namespace Vault.Core.Abstractions;

public interface IKeyDerivation
{
    byte[] DeriveKey(byte[] password, byte[] salt);
}
