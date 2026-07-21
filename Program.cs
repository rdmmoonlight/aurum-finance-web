using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using AurumFinance.Models;
using AurumFinance.Services;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext (SQLite Database) — no longer used for authentication; see
// Controllers/AuthController.cs. Kept registered for any other local data
// this app may need later.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MVC Controllers with Views
builder.Services.AddControllersWithViews();

// Typed HttpClient for Aurum.Api's Authentication feature (register, login,
// refresh, logout, forgot/reset password, verify email). "Api:BaseUrl" must
// point at wherever aurum-finance-backend is running — see appsettings.json.
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration \"Api:BaseUrl\" is required (see appsettings.json).");
builder.Services.AddHttpClient<IAurumApiClient, AurumApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// Authentication: this app authenticates users via a local cookie, not a
// JWT bearer scheme directly — the JWT lives on the backend side of the
// wire. On login/register, Controllers/AuthController.cs calls the backend
// API and stores its access + refresh tokens inside this cookie's
// AuthenticationProperties (see AuthController.SignInAsync). Services/
// CookieAuthEvents.cs then silently refreshes the access token before it
// expires, using the stored refresh token, so a session survives past the
// access token's short lifetime without another login.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = CookieAuthEvents.ValidateAsync;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Google:ClientId"] ?? "DummyClientId";
        googleOptions.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "DummyClientSecret";
        googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection(); // Hanya aktifkan HTTPS Redirection jika di Production
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
