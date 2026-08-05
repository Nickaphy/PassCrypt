# PassCrypt
A local-first, self-hosted password manager — built because I wanted one that costs nothing, has exactly the features I need, and never sends my data anywhere I don't control.

PassCrypt stores your vault as a single encrypted file on your own machine. There's no server, no account, no sync-by-default. Cloud sync may come later, but only on your own terms (e.g. syncing the encrypted file via your own storage, not a PassCrypt-hosted service).

## Status
**Early development.** Core encryption, key derivation, and a basic Blazor UI (dashboard, master password unlock) are in place.

## Features
- Local, offline-first vault — nothing leaves your machine
- Master-password-based unlock (Argon2id-derived key)
- Authenticated encryption (AES-256-GCM) so tampering is detected, not just hidden
- Blazor-based UI for managing entries

## How it's secured
| Step | What happens |
|---|---|
| **Key derivation** | Your master password + a random per-install salt (`salt.bin`) are run through **Argon2id** to produce a 256-bit key. Same password + same salt = same key, every time — but the salt means two users with the same password never get the same key. |
| **Encryption** | The derived key encrypts your vault with **AES-256-GCM**. GCM doesn't just hide the data — it authenticates it. A wrong password or a tampered vault file fails decryption with a `CryptographicException`, rather than silently returning garbage. |
| **Storage** | The vault (`Vault.bin`) stores a random 12-byte nonce, an authentication tag, and the ciphertext. The nonce ensures identical entries never produce identical ciphertext twice. |


## Architecture
PassCrypt follows a layered/clean architecture — each project has one job and only depends on the layer below it:

```
Vault.UI              → Blazor front end (pages, components, app state)
Vault.Facade           → Public API that the UI talks to; orchestrates use cases
Vault.Application      → Use cases / commands (create entry, update entry)
Vault.Infrastructure    → Encryption, key derivation, file storage — the "how"
Vault.Core             → Domain model (VaultEntry) and core abstractions — no dependencies
```

This means, for example, the UI never touches encryption directly — it goes through the Facade, which goes through Application commands, which use Infrastructure services. Makes it straightforward to swap out storage or UI later without touching the crypto.

## Tech stack
- **C# / .NET 10.0**
- **Blazor** (Server-style Razor Components, `Microsoft.NET.Sdk.Web`)
- **System.Security.Cryptography** — AES-256-GCM
- **Konscious.Security.Cryptography.Argon2** — Argon2id key derivation

## Getting started
### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Run it
```bash
git clone https://github.com/Nickaphy/PassCrypt.git
cd PassCrypt
dotnet build
dotnet run --project Vault.UI
```

On first run, a salt file is generated for your master password. Set your master password when prompted in the UI — this derives the key used to encrypt/decrypt your vault.

## Project structure
```
PassCrypt/
├── Docs/                     # Design notes (crypto rationale, etc.)
├── Vault.Core/                # Domain model — VaultEntry, core abstractions
├── Vault.Application/         # Commands and application services
├── Vault.Facade/               # Facade layer exposed to the UI
├── Vault.Infrastructure/       # Encryption, key derivation, file storage
├── Vault.UI/                   # Blazor UI (pages, layout, app state)
└── PassCrypt.sln
```

## Roadmap
- [ ] Entry search / filtering
- [ ] Password generator
- [ ] Optional user-controlled cloud sync (bring your own storage)
- [ ] Vault export / import
- [ ] Unit test coverage for crypto and application layers

## Security disclaimer
This is a personal project and **has not been independently audited**. The cryptographic choices (Argon2id + AES-256-GCM) follow current best practice, but implementation bugs are always possible. Don't use this as your sole password manager for high-value credentials until it's had real scrutiny — treat it as a learning/portfolio project.
