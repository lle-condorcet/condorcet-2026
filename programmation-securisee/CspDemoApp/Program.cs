using System.Collections.Concurrent;
using System.Text.Json;
using CspDemoApp.Middleware;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Stockage en memoire des rapports de violation CSP
var violations = new ConcurrentBag<object>();

// Activer le middleware CSP (ajoute l'en-tete a chaque reponse)
app.UseCsp();

// Servir les fichiers statiques (wwwroot/)
app.UseStaticFiles();

// ============================================================
// Page principale - HTML genere cote serveur avec nonce dynamique
// ============================================================
app.MapGet("/", (HttpContext context) =>
{
    var nonce = context.Items["csp-nonce"]?.ToString() ?? "";
    var reportOnly = context.Request.Query.ContainsKey("report-only");

    var html = $$"""
    <!DOCTYPE html>
    <html lang="fr">
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>CSP Demo - MVP .NET</title>
        <link rel="stylesheet" href="/style.css" nonce="{{nonce}}">
    </head>
    <body>
        <header>
            <h1>Content Security Policy - Demo</h1>
            <p>Application ASP.NET Core demontrant CSP en action</p>
            <div class="status-bar">
                <span>Mode CSP :</span>
                <span id="csp-status" class="status-badge">Chargement...</span>
            </div>
            <div class="mode-links">
                <a href="/">Mode Enforce</a> |
                <a href="/?report-only">Mode Report-Only</a>
            </div>
        </header>

        <!-- Section 1: Nonce -->
        <section>
            <h2>1. Nonce dynamique</h2>
            <p>
                Le nonce de cette requete est : <code>{{nonce}}</code><br>
                Rechargez la page pour voir qu'il change a chaque fois.
            </p>
            <div class="info-box">
                Le script <code>app.js</code> est autorise car il est charge depuis <code>'self'</code>.
                Le style CSS utilise <code>nonce="{{nonce}}"</code> pour etre autorise par CSP.
            </div>
        </section>

        <!-- Section 2: Tests d'attaque -->
        <section>
            <h2>2. Tests d'attaque</h2>
            <p>Cliquez sur les boutons ci-dessous pour simuler differentes attaques. Observez la console et les DevTools du navigateur (F12).</p>
            <div class="btn-group">
                <button id="btn-test-xss" class="danger">Injecter XSS (innerHTML)</button>
                <button id="btn-test-eval" class="danger">Tester eval()</button>
                <button id="btn-test-external" class="danger">Charger script externe</button>
                <button id="btn-show-headers">Voir les en-tetes</button>
            </div>
            <div id="xss-target">Zone cible pour l'injection XSS...</div>
        </section>

        <!-- Section 3: Script inline NON autorise (pas de nonce) -->
        <section>
            <h2>3. Script inline sans nonce (BLOQUE)</h2>
            <p>
                Le script ci-dessous n'a pas de nonce. CSP devrait le bloquer.
                Ouvrez la console du navigateur pour voir l'erreur.
            </p>
            <script>
                // CE SCRIPT SERA BLOQUE PAR CSP car il n'a pas de nonce
                document.write("<b style='color:red'>Si vous voyez ce texte, CSP ne fonctionne PAS !</b>");
            </script>
            <div class="info-box">
                Si rien ne s'affiche au-dessus, c'est que CSP a correctement bloque le script inline.
                Verifiez la console du navigateur (F12) pour voir le message de violation.
            </div>
        </section>

        <!-- Section 4: Script inline AUTORISE (avec nonce) -->
        <section>
            <h2>4. Script inline avec nonce (AUTORISE)</h2>
            <p>
                Ce script a le bon nonce et sera execute normalement.
            </p>
            <div id="nonce-test-result" style="padding:0.5rem; border-radius:4px;"></div>
            <script nonce="{{nonce}}">
                var el = document.getElementById("nonce-test-result");
                el.style.background = "rgba(46, 213, 115, 0.15)";
                el.style.border = "1px solid #2ed573";
                el.style.color = "#2ed573";
                el.textContent = "Ce script inline a ete execute car il possede le bon nonce !";
            </script>
        </section>

        <!-- Section 5: Console -->
        <section>
            <h2>5. Console de log</h2>
            <div id="console-output" class="console">
                <div class="console-line info">[CSP Demo] Initialisation...</div>
            </div>
        </section>

        <!-- Section 6: Rapports de violation -->
        <section>
            <h2>6. Rapports de violation CSP</h2>
            <p>
                Les violations CSP sont envoyees au endpoint <code>/csp-report</code>.
                <a href="/csp-violations" style="color:var(--accent);">Voir les rapports collectes (JSON)</a>
            </p>
            <table id="violations-list">
                <thead>
                    <tr>
                        <th>Directive violee</th>
                        <th>URI bloquee</th>
                        <th>Page</th>
                    </tr>
                </thead>
                <tbody id="violations-body">
                </tbody>
            </table>
            <script nonce="{{nonce}}">
                fetch("/csp-violations")
                    .then(function(r) { return r.json(); })
                    .then(function(data) {
                        var tbody = document.getElementById("violations-body");
                        if (data.length === 0) {
                            tbody.innerHTML = '<tr><td colspan="3" style="color:var(--text-muted)">Aucune violation enregistree. Lancez des tests ci-dessus.</td></tr>';
                            return;
                        }
                        data.forEach(function(v) {
                            var row = document.createElement("tr");
                            row.innerHTML =
                                "<td>" + (v.violatedDirective || "-") + "</td>" +
                                "<td>" + (v.blockedUri || "-") + "</td>" +
                                "<td>" + (v.documentUri || "-") + "</td>";
                            tbody.appendChild(row);
                        });
                    });
            </script>
        </section>

        <footer>
            Condorcet 2026 - Programmation Securisee - MVP Content Security Policy
        </footer>

        <script src="/app.js" nonce="{{nonce}}"></script>
    </body>
    </html>
    """;

    context.Response.ContentType = "text/html; charset=utf-8";
    return Results.Content(html, "text/html");
});

// ============================================================
// Endpoint pour recevoir les rapports de violation CSP
// ============================================================
app.MapPost("/csp-report", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    try
    {
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        // Le rapport CSP est encapsule dans "csp-report"
        if (root.TryGetProperty("csp-report", out var report))
        {
            violations.Add(new
            {
                timestamp = DateTime.UtcNow,
                violatedDirective = report.TryGetProperty("violated-directive", out var vd) ? vd.GetString() : null,
                blockedUri = report.TryGetProperty("blocked-uri", out var bu) ? bu.GetString() : null,
                documentUri = report.TryGetProperty("document-uri", out var du) ? du.GetString() : null,
                originalPolicy = report.TryGetProperty("original-policy", out var op) ? op.GetString() : null,
            });
        }
    }
    catch
    {
        // Ignorer les rapports mal formes
    }

    return Results.NoContent();
});

// ============================================================
// Endpoint pour consulter les violations collectees
// ============================================================
app.MapGet("/csp-violations", () =>
{
    return Results.Json(violations.TakeLast(50));
});

app.Run();
