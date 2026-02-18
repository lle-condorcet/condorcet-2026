using System.Security.Cryptography;

namespace CspDemoApp.Middleware;

/// <summary>
/// Middleware qui ajoute l'en-tete Content-Security-Policy a chaque reponse.
/// Genere un nonce unique par requete pour autoriser les scripts inline legitimes.
/// </summary>
public class CspMiddleware
{
    private readonly RequestDelegate _next;

    public CspMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Generer un nonce cryptographiquement aleatoire (unique par requete)
        var nonceBytes = new byte[32];
        RandomNumberGenerator.Fill(nonceBytes);
        var nonce = Convert.ToBase64String(nonceBytes);

        // Stocker le nonce dans HttpContext pour qu'il soit accessible dans les pages
        context.Items["csp-nonce"] = nonce;

        // Verifier si on est en mode report-only (via query string ?report-only=true)
        var reportOnly = context.Request.Query.ContainsKey("report-only");

        // Verifier si on autorise le CDN externe (via query string ?allow-cdn)
        var allowCdn = context.Request.Query.ContainsKey("allow-cdn");

        // Construire la directive img-src selon le mode
        var imgSrc = allowCdn
            ? $"img-src 'self' data: https://cdn.pixabay.com"
            : $"img-src 'self' data:";

        // Construire la politique CSP
        var policy = string.Join("; ",
            $"default-src 'none'",
            $"script-src 'self' 'nonce-{nonce}'",
            $"style-src 'self' 'nonce-{nonce}'",
            imgSrc,
            $"font-src 'self'",
            $"connect-src 'self'",
            $"frame-ancestors 'none'",
            $"base-uri 'self'",
            $"form-action 'self'",
            $"object-src 'none'",
            $"report-uri /csp-report"
        );

        // Choisir l'en-tete selon le mode
        var headerName = reportOnly
            ? "Content-Security-Policy-Report-Only"
            : "Content-Security-Policy";

        context.Response.Headers.Append(headerName, policy);

        // Ajouter d'autres en-tetes de securite
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        await _next(context);
    }
}

/// <summary>
/// Extension pour enregistrer le middleware facilement dans le pipeline.
/// </summary>
public static class CspMiddlewareExtensions
{
    public static IApplicationBuilder UseCsp(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CspMiddleware>();
    }
}
