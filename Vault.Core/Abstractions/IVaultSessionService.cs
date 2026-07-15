namespace Vault.Core.Abstractions;

public interface IVaultSessionService
{
    Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default);
} 