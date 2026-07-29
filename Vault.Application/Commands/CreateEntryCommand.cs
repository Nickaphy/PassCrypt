namespace Vault.Application.Commands;

public sealed record CreateEntryCommand(
    string EntryName,
    string Category,
    string Url,
    string Username,
    string Password,
    string Notes,
    string Tags);
