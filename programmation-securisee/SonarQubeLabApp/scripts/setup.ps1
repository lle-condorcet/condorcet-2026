# ==============================================
# SonarQube Lab - Script d'installation (Windows)
# ==============================================

$ErrorActionPreference = "Stop"
$ProjectDir = Split-Path -Parent (Split-Path -Parent $MyInvocation.MyCommand.Path)

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  SonarQube Lab - Installation" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Verification de Docker
try {
    docker --version | Out-Null
    Write-Host "[OK] Docker detecte." -ForegroundColor Green
} catch {
    Write-Host "[ERREUR] Docker n'est pas installe." -ForegroundColor Red
    Write-Host "Installez Docker Desktop : https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

# Verification de Docker Compose
try {
    docker compose version | Out-Null
    Write-Host "[OK] Docker Compose detecte." -ForegroundColor Green
} catch {
    Write-Host "[ERREUR] Docker Compose n'est pas disponible." -ForegroundColor Red
    exit 1
}

# Configuration WSL2 vm.max_map_count (si necessaire)
Write-Host ""
Write-Host "[INFO] Configuration de vm.max_map_count dans WSL2..." -ForegroundColor Yellow
try {
    wsl -d docker-desktop sh -c "sysctl -w vm.max_map_count=262144" 2>$null
    Write-Host "[OK] vm.max_map_count configure dans WSL2." -ForegroundColor Green
} catch {
    Write-Host "[INFO] Impossible de configurer vm.max_map_count automatiquement." -ForegroundColor Yellow
    Write-Host "       Si SonarQube ne demarre pas, executez manuellement :" -ForegroundColor Yellow
    Write-Host "       wsl -d docker-desktop sh -c 'sysctl -w vm.max_map_count=262144'" -ForegroundColor White
}

# Lancement des conteneurs
Write-Host ""
Write-Host "[INFO] Demarrage de SonarQube et PostgreSQL..." -ForegroundColor Yellow
Push-Location $ProjectDir
try {
    docker compose up -d
} finally {
    Pop-Location
}

Write-Host ""
Write-Host "[INFO] Attente du demarrage de SonarQube (peut prendre 1-2 minutes)..." -ForegroundColor Yellow

$maxWait = 180
$elapsed = 0

while ($elapsed -lt $maxWait) {
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:9000/api/system/status" -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.status -eq "UP") {
            Write-Host ""
            Write-Host "==========================================" -ForegroundColor Green
            Write-Host "  SonarQube est pret !" -ForegroundColor Green
            Write-Host "==========================================" -ForegroundColor Green
            Write-Host ""
            Write-Host "  URL     : http://localhost:9000" -ForegroundColor White
            Write-Host "  Login   : admin" -ForegroundColor White
            Write-Host "  Password: admin" -ForegroundColor White
            Write-Host ""
            Write-Host "  IMPORTANT: Changez le mot de passe admin" -ForegroundColor Yellow
            Write-Host "  au premier login !" -ForegroundColor Yellow
            Write-Host "==========================================" -ForegroundColor Green
            exit 0
        }
    } catch {
        # SonarQube pas encore pret
    }
    Start-Sleep -Seconds 5
    $elapsed += 5
    Write-Host "  ...en attente ($elapsed/${maxWait}s)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[ATTENTION] SonarQube n'est pas encore pret apres ${maxWait}s." -ForegroundColor Yellow
Write-Host "Verifiez les logs : docker compose logs sonarqube" -ForegroundColor Yellow
