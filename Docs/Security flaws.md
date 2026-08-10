🟢 Minor
  - VaultEntry.cs: constructor and UpdateDetails duplicate the same validation/normalization block almost verbatim — not SRP, but worth a private
  Normalize(...) helper to kill the duplication.
  - MasterPasswordModal.razor: injects IVaultFileStore directly just to check Exists() for first-run detection — a UI component reaching past the Facade
  into Infrastructure. Small leak, easy fix (expose IsFirstRun via the facade or session service instead).

  My actual take: don't do a big-bang refactor. Fix Dashboard.razor first — it's 80% of the pain and the only thing that's genuinely hard to reason about
  right now. VaultSessionService is worth splitting before you add more features that persist data, but it isn't urgent. Everything else is polish.

  Want me to start with the Dashboard.razor split?
