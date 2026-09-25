using Vault.Core;

namespace Vault.Core.Abstractions;

public interface IKeyDerivation
{
    byte[] DeriveKey(byte[] password, KdfParams kdf);
}
