# =============================================================================
# Import-KibanaDashboards.ps1
# Cree automatiquement le Data View, les visualisations et le dashboard
# dans Kibana pour le lab ELK Logging.
# Usage : .\import-dashboards.ps1 [-KibanaUrl http://localhost:5601]
# =============================================================================
param(
    [string]$KibanaUrl = "http://localhost:5601"
)

$headers = @{
    "kbn-xsrf"     = "true"
    "Content-Type" = "application/json"
}

# ===========================================================================
# Attente de Kibana
# ===========================================================================
function Wait-ForKibana {
    Write-Host "Attente du demarrage de Kibana..." -ForegroundColor Yellow
    $attempts = 0
    while ($attempts -lt 60) {
        try {
            $resp = Invoke-RestMethod -Uri "$KibanaUrl/api/status" -Method Get -ErrorAction Stop
            if ($resp.status.overall.level -eq "available") {
                Write-Host "Kibana est pret !" -ForegroundColor Green
                return
            }
        } catch { }
        $attempts++
        Write-Host "  Kibana pas encore pret... (tentative $attempts/60)"
        Start-Sleep -Seconds 5
    }
    Write-Host "ERREUR : Kibana n'est pas disponible apres 5 minutes." -ForegroundColor Red
    exit 1
}

# ===========================================================================
# 1. Data View
# ===========================================================================
function Get-OrCreateDataView {
    Write-Host "`n[1/6] Creation du Data View 'elklab-logs-*'..." -ForegroundColor Cyan

    $body = @{
        data_view = @{
            title         = "elklab-logs-*"
            timeFieldName = "@timestamp"
            name          = "ELK Lab Logs"
        }
    } | ConvertTo-Json -Depth 5

    try {
        $resp = Invoke-RestMethod -Uri "$KibanaUrl/api/data_views/data_view" `
            -Method Post -Headers $headers -Body $body -ErrorAction Stop
        $dvId = $resp.data_view.id
        Write-Host "  Data View cree (ID: $dvId)" -ForegroundColor Green
        return $dvId
    } catch {
        Write-Host "  Data View existe peut-etre deja, recuperation..." -ForegroundColor Yellow
        try {
            $existing = Invoke-RestMethod -Uri "$KibanaUrl/api/data_views" -Method Get -Headers $headers
            $match = $existing.data_view | Where-Object { $_.title -eq "elklab-logs-*" } | Select-Object -First 1
            if ($match) {
                Write-Host "  Data View existant (ID: $($match.id))" -ForegroundColor Green
                return $match.id
            }
            $first = $existing.data_view | Select-Object -First 1
            if ($first) {
                Write-Host "  Utilisation du premier Data View (ID: $($first.id))" -ForegroundColor Yellow
                return $first.id
            }
        } catch { }
        Write-Host "  ERREUR : impossible de creer/trouver le Data View." -ForegroundColor Red
        exit 1
    }
}

# ===========================================================================
# Helper : Creer/mettre a jour un saved object
# ===========================================================================
function Set-SavedObject {
    param(
        [string]$Type,
        [string]$Id,
        [string]$JsonBody
    )

    try {
        Invoke-RestMethod -Uri "$KibanaUrl/api/saved_objects/$Type/$Id?overwrite=true" `
            -Method Post -Headers $headers -Body $JsonBody -ErrorAction Stop | Out-Null
        return $true
    } catch {
        try {
            Invoke-RestMethod -Uri "$KibanaUrl/api/saved_objects/$Type/$Id" `
                -Method Delete -Headers $headers -ErrorAction SilentlyContinue | Out-Null
            Start-Sleep -Milliseconds 300
            Invoke-RestMethod -Uri "$KibanaUrl/api/saved_objects/$Type/$Id" `
                -Method Post -Headers $headers -Body $JsonBody -ErrorAction Stop | Out-Null
            return $true
        } catch {
            Write-Host "    Erreur sur $Type/$Id : $($_.Exception.Message)" -ForegroundColor Red
            return $false
        }
    }
}

# ===========================================================================
# 2. Latence HTTP (metric)
# ===========================================================================
function New-HttpLatencyVis {
    param([string]$DataViewId)
    Write-Host "[2/6] Creation 'Latence HTTP'..." -ForegroundColor Cyan

    # visState doit etre une chaine JSON dans attributes
    $visStateObj = @{
        title  = "Latence HTTP - Distribution"
        type   = "metric"
        aggs   = @(
            @{
                id      = "1"
                enabled = $true
                type    = "avg"
                params  = @{ field = "fields.Duration"; customLabel = "Duree moyenne (ms)" }
                schema  = "metric"
            },
            @{
                id      = "2"
                enabled = $true
                type    = "max"
                params  = @{ field = "fields.Duration"; customLabel = "Duree max (ms)" }
                schema  = "metric"
            },
            @{
                id      = "3"
                enabled = $true
                type    = "count"
                params  = @{ customLabel = "Nombre de requetes" }
                schema  = "metric"
            }
        )
        params = @{
            addTooltip     = $true
            addLegend      = $false
            type           = "metric"
            metric         = @{
                percentageMode   = $false
                useRanges        = $false
                colorSchema      = "Green to Red"
                metricColorMode  = "None"
                colorsRange      = @(
                    @{ from = 0; to = 10000 }
                )
                labels           = @{ show = $true }
                invertColors     = $false
                style            = @{
                    bgFill   = "#000"
                    bgColor  = $false
                    labelColor = $false
                    subText  = ""
                    fontSize = 60
                }
            }
        }
    }
    $visStateJson = $visStateObj | ConvertTo-Json -Depth 20 -Compress

    $searchSourceObj = @{
        query        = @{ query = "messageTemplate:*completed*"; language = "kuery" }
        filter       = @()
        indexRefName  = "kibanaSavedObjectMeta.searchSourceJSON.index"
    }
    $searchSourceJson = $searchSourceObj | ConvertTo-Json -Depth 5 -Compress

    $body = @{
        attributes = @{
            title                 = "Latence HTTP - Distribution"
            description           = "Duree moyenne et max des requetes HTTP"
            visState              = $visStateJson
            kibanaSavedObjectMeta = @{ searchSourceJSON = $searchSourceJson }
        }
        references = @(
            @{ id = $DataViewId; name = "kibanaSavedObjectMeta.searchSourceJSON.index"; type = "index-pattern" }
        )
    } | ConvertTo-Json -Depth 20

    $ok = Set-SavedObject -Type "visualization" -Id "elklab-http-latency" -JsonBody $body
    if ($ok) { Write-Host "  OK" -ForegroundColor Green }
}

# ===========================================================================
# 3. Taux d'erreurs (metric)
# ===========================================================================
function New-ErrorRateVis {
    param([string]$DataViewId)
    Write-Host "[3/6] Creation 'Taux d erreurs'..." -ForegroundColor Cyan

    $visStateObj = @{
        title  = "Erreurs recentes"
        type   = "metric"
        aggs   = @(
            @{
                id      = "1"
                enabled = $true
                type    = "count"
                params  = @{}
                schema  = "metric"
            }
        )
        params = @{
            addTooltip     = $true
            addLegend      = $false
            type           = "metric"
            metric         = @{
                percentageMode   = $false
                useRanges        = $true
                colorSchema      = "Green to Red"
                metricColorMode  = "Background"
                colorsRange      = @(
                    @{ from = 0; to = 10 },
                    @{ from = 10; to = 50 },
                    @{ from = 50; to = 10000 }
                )
                labels           = @{ show = $true }
                invertColors     = $false
                style            = @{
                    bgFill   = "#000"
                    bgColor  = $false
                    labelColor = $false
                    subText  = "Erreurs (Error + Fatal)"
                    fontSize = 60
                }
            }
        }
    }
    $visStateJson = $visStateObj | ConvertTo-Json -Depth 20 -Compress

    $searchSourceObj = @{
        query        = @{ query = "level:Error OR level:Fatal"; language = "kuery" }
        filter       = @()
        indexRefName  = "kibanaSavedObjectMeta.searchSourceJSON.index"
    }
    $searchSourceJson = $searchSourceObj | ConvertTo-Json -Depth 5 -Compress

    $body = @{
        attributes = @{
            title                 = "Erreurs recentes"
            description           = "Nombre total d'erreurs Error et Fatal"
            visState              = $visStateJson
            kibanaSavedObjectMeta = @{ searchSourceJSON = $searchSourceJson }
        }
        references = @(
            @{ id = $DataViewId; name = "kibanaSavedObjectMeta.searchSourceJSON.index"; type = "index-pattern" }
        )
    } | ConvertTo-Json -Depth 20

    $ok = Set-SavedObject -Type "visualization" -Id "elklab-error-rate" -JsonBody $body
    if ($ok) { Write-Host "  OK" -ForegroundColor Green }
}

# ===========================================================================
# 4. Securite (metric)
# ===========================================================================
function New-SecurityVis {
    param([string]$DataViewId)
    Write-Host "[4/6] Creation 'Evenements securite'..." -ForegroundColor Cyan

    $visStateObj = @{
        title  = "Alertes de securite"
        type   = "metric"
        aggs   = @(
            @{
                id      = "1"
                enabled = $true
                type    = "count"
                params  = @{}
                schema  = "metric"
            }
        )
        params = @{
            addTooltip     = $true
            addLegend      = $false
            type           = "metric"
            metric         = @{
                percentageMode   = $false
                useRanges        = $true
                colorSchema      = "Green to Red"
                metricColorMode  = "Background"
                colorsRange      = @(
                    @{ from = 0; to = 1 },
                    @{ from = 1; to = 20 },
                    @{ from = 20; to = 100000 }
                )
                labels           = @{ show = $true }
                invertColors     = $false
                style            = @{
                    bgFill   = "#000"
                    bgColor  = $false
                    labelColor = $false
                    subText  = "Evenements securite (SQLi, XSS, Rate limit)"
                    fontSize = 60
                }
            }
        }
    }
    $visStateJson = $visStateObj | ConvertTo-Json -Depth 20 -Compress

    $searchSourceObj = @{
        query        = @{ query = "message:*SECURITY*"; language = "kuery" }
        filter       = @()
        indexRefName  = "kibanaSavedObjectMeta.searchSourceJSON.index"
    }
    $searchSourceJson = $searchSourceObj | ConvertTo-Json -Depth 5 -Compress

    $body = @{
        attributes = @{
            title                 = "Alertes de securite"
            description           = "Nombre d'evenements de securite detectes"
            visState              = $visStateJson
            kibanaSavedObjectMeta = @{ searchSourceJSON = $searchSourceJson }
        }
        references = @(
            @{ id = $DataViewId; name = "kibanaSavedObjectMeta.searchSourceJSON.index"; type = "index-pattern" }
        )
    } | ConvertTo-Json -Depth 20

    $ok = Set-SavedObject -Type "visualization" -Id "elklab-security-events" -JsonBody $body
    if ($ok) { Write-Host "  OK" -ForegroundColor Green }
}

# ===========================================================================
# 5. Requetes lentes (metric)
# ===========================================================================
function New-SlowQueriesVis {
    param([string]$DataViewId)
    Write-Host "[5/6] Creation 'Requetes lentes'..." -ForegroundColor Cyan

    $visStateObj = @{
        title  = "Requetes lentes"
        type   = "metric"
        aggs   = @(
            @{
                id      = "1"
                enabled = $true
                type    = "count"
                params  = @{}
                schema  = "metric"
            }
        )
        params = @{
            addTooltip     = $true
            addLegend      = $false
            type           = "metric"
            metric         = @{
                percentageMode   = $false
                useRanges        = $true
                colorSchema      = "Green to Red"
                metricColorMode  = "Background"
                colorsRange      = @(
                    @{ from = 0; to = 5 },
                    @{ from = 5; to = 20 },
                    @{ from = 20; to = 100000 }
                )
                labels           = @{ show = $true }
                invertColors     = $false
                style            = @{
                    bgFill   = "#000"
                    bgColor  = $false
                    labelColor = $false
                    subText  = "Requetes lentes (EF > 100ms, HTTP > 500ms)"
                    fontSize = 60
                }
            }
        }
    }
    $visStateJson = $visStateObj | ConvertTo-Json -Depth 20 -Compress

    $searchSourceObj = @{
        query        = @{ query = "message:*SLOW*"; language = "kuery" }
        filter       = @()
        indexRefName  = "kibanaSavedObjectMeta.searchSourceJSON.index"
    }
    $searchSourceJson = $searchSourceObj | ConvertTo-Json -Depth 5 -Compress

    $body = @{
        attributes = @{
            title                 = "Requetes lentes"
            description           = "Nombre de requetes lentes detectees"
            visState              = $visStateJson
            kibanaSavedObjectMeta = @{ searchSourceJSON = $searchSourceJson }
        }
        references = @(
            @{ id = $DataViewId; name = "kibanaSavedObjectMeta.searchSourceJSON.index"; type = "index-pattern" }
        )
    } | ConvertTo-Json -Depth 20

    $ok = Set-SavedObject -Type "visualization" -Id "elklab-slow-queries" -JsonBody $body
    if ($ok) { Write-Host "  OK" -ForegroundColor Green }
}

# ===========================================================================
# 6. Dashboard principal
# ===========================================================================
function New-MainDashboard {
    Write-Host "[6/6] Creation du dashboard principal..." -ForegroundColor Cyan

    $panels = @(
        @{ panelIndex = "1"; gridData = @{ x = 0;  y = 0;  w = 24; h = 12; i = "1" }; type = "visualization"; panelRefName = "panel_0" }
        @{ panelIndex = "2"; gridData = @{ x = 24; y = 0;  w = 24; h = 12; i = "2" }; type = "visualization"; panelRefName = "panel_1" }
        @{ panelIndex = "3"; gridData = @{ x = 0;  y = 12; w = 24; h = 12; i = "3" }; type = "visualization"; panelRefName = "panel_2" }
        @{ panelIndex = "4"; gridData = @{ x = 24; y = 12; w = 24; h = 12; i = "4" }; type = "visualization"; panelRefName = "panel_3" }
    )

    $body = @{
        attributes = @{
            title          = "ELK Lab - Vue d'ensemble"
            description    = "Dashboard principal : latence HTTP, erreurs, securite, requetes lentes"
            panelsJSON     = ($panels | ConvertTo-Json -Depth 10 -Compress)
            optionsJSON    = (@{ useMargins = $true; syncColors = $true; hidePanelTitles = $false } | ConvertTo-Json -Compress)
            timeRestore    = $false
            kibanaSavedObjectMeta = @{
                searchSourceJSON = (@{ query = @{ query = ""; language = "kuery" }; filter = @() } | ConvertTo-Json -Depth 5 -Compress)
            }
        }
        references = @(
            @{ id = "elklab-http-latency";   name = "panel_0"; type = "visualization" }
            @{ id = "elklab-error-rate";      name = "panel_1"; type = "visualization" }
            @{ id = "elklab-security-events"; name = "panel_2"; type = "visualization" }
            @{ id = "elklab-slow-queries";    name = "panel_3"; type = "visualization" }
        )
    } | ConvertTo-Json -Depth 20

    $ok = Set-SavedObject -Type "dashboard" -Id "elklab-main-dashboard" -JsonBody $body
    if ($ok) { Write-Host "  OK" -ForegroundColor Green }
}

# ===========================================================================
# Execution
# ===========================================================================
Write-Host "=============================================" -ForegroundColor White
Write-Host "  Import des dashboards Kibana - ELK Lab" -ForegroundColor White
Write-Host "  Kibana : $KibanaUrl" -ForegroundColor White
Write-Host "=============================================" -ForegroundColor White

Wait-ForKibana

$dataViewId = Get-OrCreateDataView

New-HttpLatencyVis   -DataViewId $dataViewId
New-ErrorRateVis     -DataViewId $dataViewId
New-SecurityVis      -DataViewId $dataViewId
New-SlowQueriesVis   -DataViewId $dataViewId
New-MainDashboard

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  Import termine !" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "  1. Latence HTTP      - avg/max/count (metric)" -ForegroundColor White
Write-Host "  2. Erreurs recentes  - count avec seuils couleur (metric)" -ForegroundColor White
Write-Host "  3. Alertes securite  - count SQLi/XSS/rate limit (metric)" -ForegroundColor White
Write-Host "  4. Requetes lentes   - count slow queries (metric)" -ForegroundColor White
Write-Host "  5. Dashboard principal (4 panneaux)" -ForegroundColor White
Write-Host ""
Write-Host "Ouvrir : $KibanaUrl/app/dashboards#/view/elklab-main-dashboard" -ForegroundColor Yellow
Write-Host ""
