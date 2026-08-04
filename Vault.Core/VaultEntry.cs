using System.Text.Json.Serialization;

namespace Vault.Core;

public class VaultEntry
{
    [JsonInclude]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [JsonInclude]
    public string Category { get; private set; } = string.Empty;

    [JsonInclude]
    public string EntryName { get; private set; } = string.Empty;

    [JsonInclude]
    public string Url { get; private set; } = string.Empty;

    [JsonInclude]
    public string Username { get; private set; } = string.Empty;

    [JsonInclude]
    public string Password { get; private set; } = string.Empty;

    [JsonInclude]
    public string Notes { get; private set; } = string.Empty;

    [JsonInclude]
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
    
    
    // Core method for update only because it mutates existing entries.
    // We do this because AddEntry creates a new vault entry through the constructor which ensures it follows core rules
    // Update doesn't create a new vaultentry so it has to have a core method to prevent skipping or rule dupes.
    public void UpdateDetails(
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        if (string.IsNullOrWhiteSpace(entryName))
            throw new ArgumentException("Entry name is required.", nameof(entryName));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));

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
