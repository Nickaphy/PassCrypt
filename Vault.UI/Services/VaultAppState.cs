namespace Vault.UI.Services;

public class VaultAppState
{
    public bool IsUnlocked { get; private set; }

    public event Action? OnChange;

    public void SetUnlocked()
    {
        IsUnlocked = true;
        OnChange?.Invoke();
    }
}
