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
// Entry application service.
builder.Services.AddScoped<IEntryApplicationService, EntryApplicationService>();
// Facade.
builder.Services.AddScoped<IVaultFacade, VaultFacade>();

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