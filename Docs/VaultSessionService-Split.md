# Splitting `VaultSessionService`

## The problem

`Vault.Infrastructure/Services/VaultSessionService.cs` currently does three unrelated jobs in one class:

1. **Session/crypto lifecycle** — derives the AES key from the master password, holds it in memory, clears it on lock.
2. **In-memory entry management** — owns the `List<VaultEntry>` and does add/update/delete/get against it.
3. **Persistence** — turns that list into encrypted bytes and writes/reads them via `IVaultFileStore`.

Because all three live in one class, you can't touch one without dragging the others along — e.g. you can't test "does `AddEntry` reject a duplicate" without also running real AES-GCM encryption and writing a real file to disk.

## The target file layout

Four files in `Vault.Infrastructure/Services/`, replacing the one:

```
Vault.Infrastructure/Services/
├── VaultKeySession.cs         (new)
├── VaultEntryRepository.cs    (new)
├── VaultPersistenceService.cs (new)
└── VaultSessionService.cs     (rewritten — becomes a thin coordinator)
```

**Nothing outside `Vault.Infrastructure` changes.** `IVaultSessionService` (in `Vault.Core.Abstractions`) keeps its exact current shape, so `EntryApplicationService`, `VaultEntryQueryService`, and `MasterPasswordModal.razor` don't need to know any of this happened.

---

## 1. `VaultKeySession.cs`

**Responsibility:** *"What is the current session's cryptographic key, and am I allowed to hand it out?"* Nothing about entries, nothing about files.

**Moves in from `VaultSessionService`:**
- Field `_sessionKey`
- Field `_loadCreateSalt` (the `LoadCreateSalt` dependency)
- Field `_keyDerivation` (the `KeyDerivation` dependency)
- The salt-load + derive lines from `UnlockWithPasswordAsync`:
  ```csharp
  byte[] salt = _loadCreateSalt.LoadOrCreateSalt();
  _sessionKey = _keyDerivation.DeriveKey(masterPassword, salt);
  ```
- The key-clearing lines from `LockVault()`:
  ```csharp
  if (_sessionKey is not null)
  {
      Array.Clear(_sessionKey, 0, _sessionKey.Length);
      _sessionKey = null;
  }
  ```
- The `try/finally` that clears the incoming `masterPassword` bytes — this moves here too, because the password is only ever consumed during derivation. Today that `finally` sits in `VaultSessionService.UnlockWithPasswordAsync` wrapping the *whole* unlock flow (derivation **and** entry loading); once derivation is its own method, the clearing belongs directly around it, not around unrelated work happening afterward.
- The five copies of `if (_sessionKey == null) throw new UnauthorizedAccessException("Vault is locked.")` scattered across `AddEntry`, `DeleteEntry`, `UpdateEntry`, `GetEntries`, and `SaveVault` collapse into **one** method here: `RequireSessionKey()`.

**Full file:**

```csharp
using Vault.Infrastructure;

namespace Vault.Infrastructure.Services;

public class VaultKeySession
{
    private readonly LoadCreateSalt _loadCreateSalt;
    private readonly KeyDerivation _keyDerivation;

    private byte[]? _sessionKey;

    public VaultKeySession(LoadCreateSalt loadCreateSalt, KeyDerivation keyDerivation)
    {
        _loadCreateSalt = loadCreateSalt;
        _keyDerivation = keyDerivation;
    }

    public bool IsUnlocked => _sessionKey is not null;

    // Derives the session key from the master password and clears the
    // password bytes immediately afterward, whether derivation succeeded or not.
    public void Derive(byte[] masterPassword)
    {
        try
        {
            byte[] salt = _loadCreateSalt.LoadOrCreateSalt();
            _sessionKey = _keyDerivation.DeriveKey(masterPassword, salt);
        }
        finally
        {
            Array.Clear(masterPassword, 0, masterPassword.Length);
        }
    }

    public byte[] RequireSessionKey()
    {
        if (_sessionKey is null)
            throw new UnauthorizedAccessException("Vault is locked.");

        return _sessionKey;
    }

    public void Clear()
    {
        if (_sessionKey is not null)
        {
            Array.Clear(_sessionKey, 0, _sessionKey.Length);
            _sessionKey = null;
        }
    }
}
```

**Stays out:** `_entries`, `_decryptor`, `_encryptor`, `_vaultFileStore` — none of those are this class's concern.

---

## 2. `VaultEntryRepository.cs`

**Responsibility:** *"What entries currently exist in memory?"* No crypto, no file I/O — just a list and the rules for mutating it.

**Moves in from `VaultSessionService`:**
- Field `_entries`
- The body of `AddEntry` (minus the lock guard)
- The body of `DeleteEntry` (minus the lock guard)
- The body of `UpdateEntry` (minus the lock guard)
- The body of `GetEntries` (minus the lock guard)
- The `_entries.Clear()` line from `LockVault()`
- The "find or throw `InvalidOperationException`" logic that was duplicated inside `DeleteEntry` and `UpdateEntry` collapses into one private helper, `FindOrThrow`.

**New method not present before:** `ReplaceAll(IEnumerable<VaultEntry>)` — used once, right after unlock, to load persisted entries into memory. Previously this was just a raw field assignment (`_entries = ...`) inline in `UnlockWithPasswordAsync`; now that `_entries` is private to this class, it needs an explicit method.

**Full file:**

```csharp
using Vault.Core;

namespace Vault.Infrastructure.Services;

public class VaultEntryRepository
{
    private List<VaultEntry> _entries = new();

    public void ReplaceAll(IEnumerable<VaultEntry> entries)
    {
        _entries = entries.ToList();
    }

    public IReadOnlyList<VaultEntry> GetAll() => _entries.ToList();

    public void Add(VaultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries.Add(entry);
    }

    public void Delete(Guid entryId)
    {
        VaultEntry entry = FindOrThrow(entryId);
        _entries.Remove(entry);
    }

    public void Update(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        VaultEntry entry = FindOrThrow(entryId);
        entry.UpdateDetails(entryName, category, url, username, password, notes, tags);
    }

    public void Clear() => _entries.Clear();

    private VaultEntry FindOrThrow(Guid entryId)
    {
        VaultEntry? entry = _entries.FirstOrDefault(e => e.Id == entryId);
        if (entry is null)
            throw new InvalidOperationException($"Entry with ID '{entryId}' was not found.");

        return entry;
    }
}
```

**Stays out:** any notion of "session key", "locked", `Encryptor`/`Decryptor`, `IVaultFileStore`. This class doesn't know encryption exists.

---

## 3. `VaultPersistenceService.cs`

**Responsibility:** *"How does the in-memory entry list get to and from disk?"* Owns the JSON serialization and the encrypt/decrypt round trip around `IVaultFileStore`.

**Moves in from `VaultSessionService`:**
- Field `_decryptor`
- Field `_encryptor`
- Field `_vaultFileStore`
- The entire private `SaveVault()` method becomes `Save(byte[] sessionKey, IReadOnlyList<VaultEntry> entries)` — it no longer reads `_entries`/`_sessionKey` off shared state, they're passed in as parameters.
- The load/decrypt/deserialize lines from `UnlockWithPasswordAsync`:
  ```csharp
  var (nonce, tag, cipherText) = _vaultFileStore.Load();
  byte[] decryptedBytes = _decryptor.Decrypt(_sessionKey, nonce, tag, cipherText);
  string json = System.Text.Encoding.UTF8.GetString(decryptedBytes);
  _entries = JsonSerializer.Deserialize<List<VaultEntry>>(json) ?? new List<VaultEntry>();
  ```
  become a new `Load(byte[] sessionKey)` method that returns the list instead of assigning a field.
- The `_vaultFileStore.Exists()` check from `UnlockWithPasswordAsync` becomes a passthrough method `VaultExists()`, so `VaultSessionService` never needs to know `IVaultFileStore` exists at all.

**Full file:**

```csharp
using System.Text;
using System.Text.Json;
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultPersistenceService
{
    private readonly Encryptor _encryptor;
    private readonly Decryptor _decryptor;
    private readonly IVaultFileStore _vaultFileStore;

    public VaultPersistenceService(Encryptor encryptor, Decryptor decryptor, IVaultFileStore vaultFileStore)
    {
        _encryptor = encryptor;
        _decryptor = decryptor;
        _vaultFileStore = vaultFileStore;
    }

    public bool VaultExists() => _vaultFileStore.Exists();

    public void Save(byte[] sessionKey, IReadOnlyList<VaultEntry> entries)
    {
        string json = JsonSerializer.Serialize(entries);
        byte[] plainText = Encoding.UTF8.GetBytes(json);

        byte[] cipherText = _encryptor.Encrypt(sessionKey, plainText, out byte[] nonce, out byte[] tag);

        _vaultFileStore.Save(nonce, tag, cipherText);
    }

    public List<VaultEntry> Load(byte[] sessionKey)
    {
        var (nonce, tag, cipherText) = _vaultFileStore.Load();

        byte[] decryptedBytes = _decryptor.Decrypt(sessionKey, nonce, tag, cipherText);
        string json = Encoding.UTF8.GetString(decryptedBytes);

        return JsonSerializer.Deserialize<List<VaultEntry>>(json) ?? new List<VaultEntry>();
    }
}
```

**Stays out:** anything about *when* to save (that's `VaultSessionService`'s job as coordinator) — this class just does the mechanical translation when told to.

---

## 4. `VaultSessionService.cs` (rewritten)

**Responsibility:** coordinate the other three. It still implements `IVaultSessionService`, so it's still the only thing the rest of the app talks to — but now it just wires calls together instead of doing the work itself.

```csharp
using Vault.Core;
using Vault.Core.Abstractions;

namespace Vault.Infrastructure.Services;

public class VaultSessionService : IVaultSessionService
{
    private readonly VaultKeySession _keySession;
    private readonly VaultEntryRepository _entryRepository;
    private readonly VaultPersistenceService _persistenceService;

    public VaultSessionService(
        VaultKeySession keySession,
        VaultEntryRepository entryRepository,
        VaultPersistenceService persistenceService)
    {
        _keySession = keySession;
        _entryRepository = entryRepository;
        _persistenceService = persistenceService;
    }

    public Task UnlockWithPasswordAsync(byte[] masterPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _keySession.Derive(masterPassword);
        byte[] sessionKey = _keySession.RequireSessionKey();

        if (!_persistenceService.VaultExists())
        {
            _entryRepository.Clear();
            _persistenceService.Save(sessionKey, _entryRepository.GetAll());
        }
        else
        {
            List<VaultEntry> entries = _persistenceService.Load(sessionKey);
            _entryRepository.ReplaceAll(entries);
        }

        return Task.CompletedTask;
    }

    public void AddEntry(VaultEntry entry)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Add(entry);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public void DeleteEntry(Guid entryId)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Delete(entryId);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public void UpdateEntry(
        Guid entryId,
        string entryName,
        string category,
        string url,
        string username,
        string password,
        string notes,
        IEnumerable<string> tags)
    {
        byte[] sessionKey = _keySession.RequireSessionKey();
        _entryRepository.Update(entryId, entryName, category, url, username, password, notes, tags);
        _persistenceService.Save(sessionKey, _entryRepository.GetAll());
    }

    public IReadOnlyList<VaultEntry> GetEntries()
    {
        _keySession.RequireSessionKey();
        return _entryRepository.GetAll();
    }

    public void LockVault()
    {
        _keySession.Clear();
        _entryRepository.Clear();
    }
}
```

Notice every method now follows the same shape: **guard → delegate → (maybe) persist**. That repetition is fine — it's the coordinator's whole job, and it's now the *only* thing left in this file.

---

## Wiring it up in `Program.cs`

Three new registrations, alongside the existing ones (which stay — the new classes still need `LoadCreateSalt`, `KeyDerivation`, `Encryptor`, `Decryptor`, `IVaultFileStore` as their own dependencies):

```csharp
builder.Services.AddScoped<VaultKeySession>();
builder.Services.AddScoped<VaultEntryRepository>();
builder.Services.AddScoped<VaultPersistenceService>();
```

`builder.Services.AddScoped<IVaultSessionService, VaultSessionService>();` stays exactly as it is — the DI container resolves the three new constructor parameters automatically.

---

## What doesn't change

- `IVaultSessionService` — same five members, same signatures.
- `EntryApplicationService`, `VaultEntryQueryService` — still depend only on `IVaultSessionService`, unaware any of this happened.
- `MasterPasswordModal.razor` — still injects `IVaultSessionService` and `IVaultFileStore` exactly as before.
- Behavior — this is a pure internal restructuring. Same inputs should produce the same outputs and the same exceptions.

## Why this is worth it

Each new class can now be understood and tested on its own:
- `VaultEntryRepository`'s "throws when entry not found" or "rejects null entry" logic can be tested with zero crypto and zero disk I/O.
- `VaultPersistenceService`'s save/load round trip can be tested against a fake `IVaultFileStore` without touching key derivation.
- `VaultKeySession`'s "locked vs unlocked" state can be tested without entries or files existing at all.

`VaultSessionService` itself becomes small enough to read top to bottom in a few seconds and see exactly what happens on unlock, add, delete, update, and lock — no more hunting through one method to figure out which of three concerns a given line belongs to.
