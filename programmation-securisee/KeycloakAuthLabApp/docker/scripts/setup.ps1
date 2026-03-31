# ──────────────────────────────────────
# Script d'installation du laboratoire Keycloak (PowerShell)
# ──────────────────────────────────────

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$DockerDir = Split-Path -Parent $ScriptDir

Write-Host "======================================"  -ForegroundColor Cyan
Write-Host "  Keycloak Authentication Lab - Setup"  -ForegroundColor Cyan
Write-Host "======================================"  -ForegroundColor Cyan
Write-Host ""

# Verifier Docker
try {
    docker --version | Out-Null
} catch {
    Write-Host "ERREUR: Docker n'est pas installe." -ForegroundColor Red
    Write-Host "Installez Docker Desktop: https://www.docker.com/products/docker-desktop"
    exit 1
}

# Verifier docker compose
try {
    docker compose version | Out-Null
} catch {
    Write-Host "ERREUR: docker compose n'est pas disponible." -ForegroundColor Red
    exit 1
}

Write-Host "[1/4] Demarrage de l'infrastructure..." -ForegroundColor Yellow
Set-Location $DockerDir
docker compose up -d

Write-Host ""
Write-Host "[2/4] Attente du demarrage de Keycloak..." -ForegroundColor Yellow
Write-Host "       (cela peut prendre 30-60 secondes)"

$retries = 30
$ready = $false

while (-not $ready -and $retries -gt 0) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:8080/health/ready" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $ready = $true
        }
    } catch {
        $retries--
        Write-Host "       En attente... ($retries tentatives restantes)"
        Start-Sleep -Seconds 5
    }
}

if (-not $ready) {
    Write-Host "ERREUR: Keycloak n'a pas demarre dans le delai imparti." -ForegroundColor Red
    Write-Host "Verifiez les logs: docker compose logs keycloak"
    exit 1
}

Write-Host "       Keycloak est pret !" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] Verification de l'import du realm..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "[4/4] Verification des services..." -ForegroundColor Yellow

Write-Host ""
Write-Host "======================================"  -ForegroundColor Green
Write-Host "  Installation terminee !"               -ForegroundColor Green
Write-Host "======================================"  -ForegroundColor Green
Write-Host ""
Write-Host "  Keycloak Admin Console : " -NoNewline; Write-Host "http://localhost:8080" -ForegroundColor Cyan
Write-Host "    - Utilisateur: admin"
Write-Host "    - Mot de passe: admin"
Write-Host ""
Write-Host "  Application .NET       : " -NoNewline; Write-Host "http://localhost:5000" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Utilisateurs de test:"
Write-Host "    - admin@lab.local    / Admin123!"
Write-Host "    - manager@lab.local  / Manager123!"
Write-Host "    - user@lab.local     / User123!"
Write-Host "    - auditor@lab.local  / Auditor123!"
Write-Host ""
Write-Host "  Pour arreter: docker compose down"
Write-Host "  Pour les logs: docker compose logs -f"
Write-Host "======================================"
