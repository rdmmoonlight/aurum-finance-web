using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using AurumFinance.Models;
using AurumFinance.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. MVC Services
builder.Services.AddControllersWithViews();

// 3. HTTP Client
var apiBaseUrl = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Configuration \"Api:BaseUrl\" is required (see appsettings.json).");
builder.Services.AddHttpClient<IAurumApiClient, AurumApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// 4. Authentication Configuration
// Cek kredensial dari "Authentication:Google:..." (User Secrets / Railway Env Vars)
var googleClientId = builder.Configuration["Authentication:Google:ClientId"] 
    ?? builder.Configuration["Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] 
    ?? builder.Configuration["Google:ClientSecret"];

var authBuilder = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/Login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
        options.Events.OnValidatePrincipal = CookieAuthEvents.ValidateAsync;
    });

// Daftarkan Google Auth HANYA jika ClientId dan ClientSecret valid (tidak kosong)
if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
{
    authBuilder.AddGoogle(GoogleDefaults.AuthenticationScheme, googleOptions =>
    {
        googleOptions.ClientId = googleClientId;
        googleOptions.ClientSecret = googleClientSecret;
        googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();