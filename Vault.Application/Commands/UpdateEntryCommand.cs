namespace Vault.Application.Commands;

public sealed record UpdateEntryCommand(
    Guid EntryId,
    string EntryName,
    string Category,
    string Url,
    string Username,
    string Password,
    string Notes,
    string Tags);
