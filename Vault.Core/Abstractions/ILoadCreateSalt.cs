namespace Vault.Core.Abstractions;

public interface ILoadCreateSalt
{
    byte[] LoadOrCreateSalt();
    string GetSaltPath();
}
