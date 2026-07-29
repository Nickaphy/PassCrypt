namespace Vault.Core;

public class VaultEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Category { get; private set; } = string.Empty;
    public string EntryName { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Username { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public List<string> Tags { get; private set; } = new();

    public VaultEntry()
    {
    }

    public VaultEntry(
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        if (string.IsNullOrWhiteSpace(entryName))
        {
            throw new ArgumentException("Entry name is required.", nameof(entryName));
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.", nameof(password));
        }

        EntryName = entryName.Trim();
        Category = category.Trim();
        Url = url.Trim();
        Username = username.Trim();
        Password = password;
        Notes = notes.Trim();
        Tags = tags
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
