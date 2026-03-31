using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using KeycloakAuthLabApp.Authorization;
using KeycloakAuthLabApp.Data;
using KeycloakAuthLabApp.Models;
using KeycloakAuthLabApp.Services;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────
// Database
// ──────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=keycloaklab.db"));

// ──────────────────────────────────────
// Authentication: Cookie + OpenID Connect + JWT Bearer
// ──────────────────────────────────────
var keycloakSection = builder.Configuration.GetSection("Keycloak");

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "KeycloakLabAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
})
.AddOpenIdConnect(options =>
{
    options.Authority = keycloakSection["Authority"]
        ?? "http://localhost:8080/realms/securite-lab";
    options.ClientId = keycloakSection["ClientId"] ?? "securite-lab-app";
    options.ClientSecret = keycloakSection["ClientSecret"] ?? "secret";
    options.ResponseType = OpenIdConnectResponseType.Code;
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = false; // Dev only — set true in production

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("roles");

    // Map Keycloak claims
    options.ClaimActions.MapJsonKey("preferred_username", "preferred_username");
    options.ClaimActions.MapJsonKey("realm_access", "realm_access");
    options.ClaimActions.MapJsonKey("resource_access", "resource_access");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "preferred_username",
        RoleClaimType = "role",
        ValidateIssuer = true,
        ValidIssuer = keycloakSection["Authority"]
            ?? "http://localhost:8080/realms/securite-lab"
    };

    // Handle sign-out to also log out from Keycloak
    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProviderForSignOut = context =>
        {
            var logoutUri = $"{context.Options.Authority}/protocol/openid-connect/logout";
            var postLogoutUri = context.Properties.RedirectUri ?? "/";

            if (!string.IsNullOrEmpty(postLogoutUri))
            {
                logoutUri += $"?post_logout_redirect_uri={Uri.EscapeDataString(postLogoutUri)}";
                logoutUri += $"&client_id={context.Options.ClientId}";
            }

            context.Response.Redirect(logoutUri);
            context.HandleResponse();
            return Task.CompletedTask;
        }
    };
})
.AddJwtBearer(options =>
{
    options.Authority = keycloakSection["Authority"]
        ?? "http://localhost:8080/realms/securite-lab";
    options.Audience = keycloakSection["ClientId"] ?? "securite-lab-app";
    options.RequireHttpsMetadata = false; // Dev only

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = keycloakSection["Authority"]
            ?? "http://localhost:8080/realms/securite-lab",
        ValidateAudience = true,
        ValidAudiences = new[]
        {
            keycloakSection["ClientId"] ?? "securite-lab-app",
            "account"
        },
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30)
    };
});

// Claims transformation: map Keycloak roles to .NET roles
builder.Services.AddTransient<IClaimsTransformation, KeycloakRoleTransformer>();

// ──────────────────────────────────────
// Authorization policies
// ──────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("admin"));

    options.AddPolicy("RequireManager", policy =>
        policy.RequireRole("admin", "manager"));

    options.AddPolicy("RequireUser", policy =>
        policy.RequireRole("admin", "manager", "user", "auditor"));

    // Classification-based policies
    options.AddPolicy("ClassificationPublic", policy =>
        policy.Requirements.Add(new ClassificationRequirement(Classification.Public)));

    options.AddPolicy("ClassificationInternal", policy =>
        policy.Requirements.Add(new ClassificationRequirement(Classification.Internal)));

    options.AddPolicy("ClassificationConfidential", policy =>
        policy.Requirements.Add(new ClassificationRequirement(Classification.Confidential)));

    options.AddPolicy("ClassificationSecret", policy =>
        policy.Requirements.Add(new ClassificationRequirement(Classification.Secret)));
});

builder.Services.AddSingleton<IAuthorizationHandler, ClassificationHandler>();

// ──────────────────────────────────────
// Services
// ──────────────────────────────────────
builder.Services.AddHttpClient<KeycloakAdminService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<AccessLogService>();

// ──────────────────────────────────────
// MVC + CORS
// ──────────────────────────────────────
builder.Services.AddControllersWithViews();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5173" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ──────────────────────────────────────
// Ensure database is created
// ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

// ──────────────────────────────────────
// Middleware pipeline
// ──────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
