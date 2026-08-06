using Vault.Application.Interface;
using Vault.Application.Services;
using Vault.Core.Abstractions;
using Vault.Facade.Interface;
using Vault.Facade.Services;
using Vault.Infrastructure;
using Vault.Infrastructure.Services;
using Vault.UI.Components;
using Vault.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options => options.DetailedErrors = true);

// Dependency injection for the services.
// Load create salt.
builder.Services.AddSingleton<ILoadCreateSalt, LoadCreateSalt>();
// Key derivation.
builder.Services.AddSingleton<IKeyDerivation, KeyDerivation>();
// Decryptor.
builder.Services.AddScoped<IDecryptor, Decryptor>();
// Encryptor.
builder.Services.AddScoped<IEncryptor, Encryptor>();
// Dependency injection for the vault app state.
builder.Services.AddScoped<VaultAppState>();
// Vault key session (derives + holds the session's encryption key).
builder.Services.AddScoped<IVaultKeySession, VaultKeySession>();
// Vault entry repository (in-memory entry list).
builder.Services.AddScoped<IVaultEntryRepository, VaultEntryRepository>();
// Vault persistence service (encrypt/decrypt + read/write the vault file).
builder.Services.AddScoped<IVaultPersistenceService, VaultPersistenceService>();
// Vault session service (coordinates the three services above).
builder.Services.AddScoped<IVaultSessionService, VaultSessionService>();
// Vault file store.
builder.Services.AddScoped<IVaultFileStore, VaultFileStore>();
// Entry application service.
builder.Services.AddScoped<IEntryApplicationService, EntryApplicationService>();
// Facade.
builder.Services.AddScoped<IVaultFacade, VaultFacade>();

builder.Services.AddScoped<IVaultEntryQueryService, VaultEntryQueryService>();
//Clipboard service for password and username
builder.Services.AddScoped<ClipboardService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();