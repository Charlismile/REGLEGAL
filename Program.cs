using Mapster;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using REGISTROLEGAL.Components;
using REGISTROLEGAL.Components.Account;
using REGISTROLEGAL.Data;
using REGISTROLEGAL.Models.Entities.BdSisLegal;
using REGISTROLEGAL.Repositories.Interfaces;
using REGISTROLEGAL.Repositories.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Identity and authentication services
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

// Add Authentication and Identity
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

// Get connection strings
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Register DbContexts
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// SOLUCIÓN: Registrar SOLO el DbContextFactory para DbContextLegal (no ambos)
// El factory manejará la creación de instancias del DbContext
builder.Services.AddDbContextFactory<DbContextLegal>(options =>
    options.UseSqlServer(connectionString));

// ✅ Register HttpClient
builder.Services.AddHttpClient();

// Register Mapster
builder.Services.AddMapster();

// Register custom services DESPUÉS de los DbContexts - CORREGIDO
builder.Services.AddScoped<ICommon, CommonServices>();
builder.Services.AddScoped<ILocalStorage, LocalStorageServices>();
builder.Services.AddScoped<IDatabaseProvider, DatabaseProviderService>();
builder.Services.AddScoped<IRegistroComite, RegistroComiteService>();
builder.Services.AddScoped<IRegistroAsociacion, RegistroAsociacionService>();

// Identity Core
builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

// Configuración de carga de archivos 
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 50_000_000; // 50 MB
});

var app = builder.Build();

// Configure the HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

app.Run();