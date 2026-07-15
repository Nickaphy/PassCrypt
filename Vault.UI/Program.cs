using Vault.Core.Abstractions;
using Vault.Infrastructure;
using Vault.Infrastructure.Services;
using Vault.UI.Components;
using Vault.UI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Dependency injection for the services.
// Load create salt.
builder.Services.AddSingleton<LoadCreateSalt>();
// Key derivation.
builder.Services.AddSingleton<KeyDerivation>();
// Decryptor.
builder.Services.AddScoped<Decryptor>();
// Encryptor.
builder.Services.AddScoped<Encryptor>();
// Dependency injection for the vault app state.
builder.Services.AddScoped<VaultAppState>();
// Vault session service.
builder.Services.AddScoped<IVaultSessionService, VaultSessionService>();
// Vault file store.
builder.Services.AddScoped<IVaultFileStore, VaultFileStore>();

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