using CorsLabApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// Configuration des politiques CORS
// ============================================================

builder.Services.AddCors(options =>
{
    // Politique "wildcard" : Access-Control-Allow-Origin: *
    // Permet toutes les origines mais interdit les credentials
    options.AddPolicy("Wildcard", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    // Politique "vulnerable" : reflexion d'origine + credentials
    // C'est LA faille CORS critique a demontrer
    options.AddPolicy("Vulnerable", policy =>
    {
        policy.SetIsOriginAllowed(_ => true)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    // Politique "secure" : whitelist stricte
    options.AddPolicy("Secure", policy =>
    {
        policy.WithOrigins("https://localhost:5301")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors();

// ============================================================
// Donnees bancaires simulees en memoire
// ============================================================

var compte = new CompteDto(
    Nom: "Jean Dupont",
    Iban: "BE68 5390 0754 7034",
    Solde: 12750.42m,
    Devise: "EUR"
);

var transactions = new List<TransactionDto>
{
    new("2026-02-15", "Salaire - Entreprise ABC", 3200.00m, "credit"),
    new("2026-02-14", "Loyer - Appartement", -850.00m, "debit"),
    new("2026-02-13", "Courses - Carrefour", -127.35m, "debit"),
    new("2026-02-12", "Virement - Marie Dupont", -200.00m, "debit"),
    new("2026-02-10", "Remboursement - Assurance", 45.80m, "credit"),
    new("2026-02-08", "Restaurant - Le Petit Chef", -62.50m, "debit"),
    new("2026-02-05", "Abonnement - Netflix", -17.99m, "debit"),
    new("2026-02-01", "Electricite - Engie", -95.20m, "debit")
};

// ============================================================
// Helpers d'authentification par cookie
// ============================================================

bool EstAuthentifie(HttpContext ctx)
{
    return ctx.Request.Cookies.ContainsKey("session_banque");
}

void SetSessionCookie(HttpContext ctx)
{
    ctx.Response.Cookies.Append("session_banque", "user_jean_dupont", new CookieOptions
    {
        HttpOnly = true,
        SameSite = SameSiteMode.None,
        Secure = true,
        Path = "/",
        MaxAge = TimeSpan.FromHours(1)
    });
}

void ClearSessionCookie(HttpContext ctx)
{
    ctx.Response.Cookies.Delete("session_banque", new CookieOptions
    {
        SameSite = SameSiteMode.None,
        Secure = true,
        Path = "/"
    });
}

// ============================================================
// Routes d'authentification (pas de politique CORS specifique)
// ============================================================

app.MapPost("/api/auth/login", (HttpContext ctx) =>
{
    SetSessionCookie(ctx);
    return Results.Json(new { success = true, message = "Connecte en tant que Jean Dupont" });
}).RequireCors("Vulnerable");

app.MapPost("/api/auth/logout", (HttpContext ctx) =>
{
    ClearSessionCookie(ctx);
    return Results.Json(new { success = true, message = "Deconnecte" });
}).RequireCors("Vulnerable");

app.MapGet("/api/auth/status", (HttpContext ctx) =>
{
    var auth = EstAuthentifie(ctx);
    return Results.Json(new { authenticated = auth, user = auth ? "Jean Dupont" : null as string });
}).RequireCors("Vulnerable");

// ============================================================
// Groupe 1 : Sans CORS — aucun en-tete CORS
// Le navigateur bloque la reponse (politique SOP)
// ============================================================

app.MapGet("/api/no-cors/compte", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(compte);
});

app.MapGet("/api/no-cors/transactions", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(transactions);
});

// ============================================================
// Groupe 2 : Wildcard — Access-Control-Allow-Origin: *
// Ouvert a tous mais impossible d'envoyer des credentials
// ============================================================

app.MapGet("/api/wildcard/compte", () =>
{
    return Results.Json(compte);
}).RequireCors("Wildcard");

app.MapGet("/api/wildcard/transactions", () =>
{
    return Results.Json(transactions);
}).RequireCors("Wildcard");

// ============================================================
// Groupe 3 : Vulnerable — reflexion d'origine + credentials
// Permet le vol de donnees bancaires depuis n'importe quelle origine
// ============================================================

app.MapGet("/api/vulnerable/compte", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(compte);
}).RequireCors("Vulnerable");

app.MapGet("/api/vulnerable/transactions", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(transactions);
}).RequireCors("Vulnerable");

// ============================================================
// Groupe 4 : Secure — whitelist d'origines
// Seul https://localhost:5301 est autorise
// ============================================================

app.MapGet("/api/secure/compte", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(compte);
}).RequireCors("Secure");

app.MapGet("/api/secure/transactions", (HttpContext ctx) =>
{
    if (!EstAuthentifie(ctx))
        return Results.Json(new { error = "Non authentifie" }, statusCode: 401);
    return Results.Json(transactions);
}).RequireCors("Secure");

app.Run();
