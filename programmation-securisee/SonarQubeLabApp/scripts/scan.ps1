# ==============================================
# SonarQube Lab - Script d'analyse .NET (Windows)
# ==============================================

param(
    [string]$ProjectPath = ".",
    [string]$SonarHostUrl = $env:SONAR_HOST_URL,
    [string]$SonarProjectKey = $env:SONAR_PROJECT_KEY,
    [string]$SonarToken = $env:SONAR_TOKEN
)

$ErrorActionPreference = "Stop"

# Valeurs par defaut
if (-not $SonarHostUrl) { $SonarHostUrl = "http://localhost:9000" }
if (-not $SonarProjectKey) { $SonarProjectKey = "my-dotnet-project" }

Write-Host "==========================================" -ForegroundColor Cyan
Write-Host "  SonarQube - Analyse .NET" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

# Verification du token
if (-not $SonarToken) {
    Write-Host "[ERREUR] SONAR_TOKEN n'est pas defini." -ForegroundColor Red
    Write-Host ""
    Write-Host "Generez un token dans SonarQube :" -ForegroundColor Yellow
    Write-Host "  1. Allez sur $SonarHostUrl" -ForegroundColor White
    Write-Host "  2. Mon Compte > Securite > Generer un token" -ForegroundColor White
    Write-Host "  3. Definissez : `$env:SONAR_TOKEN = 'sqp_votre_token'" -ForegroundColor White
    exit 1
}

# Verification de dotnet
try {
    dotnet --version | Out-Null
} catch {
    Write-Host "[ERREUR] .NET SDK n'est pas installe." -ForegroundColor Red
    exit 1
}

# Verification/Installation du scanner .NET
$tools = dotnet tool list -g 2>$null
if ($tools -notmatch "dotnet-sonarscanner") {
    Write-Host "[INFO] Installation de dotnet-sonarscanner..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-sonarscanner
}

Write-Host "[INFO] Analyse du projet : $ProjectPath" -ForegroundColor White
Write-Host "[INFO] Cle du projet    : $SonarProjectKey" -ForegroundColor White
Write-Host "[INFO] Serveur SonarQube: $SonarHostUrl" -ForegroundColor White
Write-Host ""

Push-Location $ProjectPath
try {
    # Debut de l'analyse
    Write-Host "[1/3] Demarrage de l'analyse SonarQube..." -ForegroundColor Yellow
    dotnet sonarscanner begin `
        /k:"$SonarProjectKey" `
        /d:sonar.host.url="$SonarHostUrl" `
        /d:sonar.token="$SonarToken"

    # Build du projet
    Write-Host ""
    Write-Host "[2/3] Build du projet .NET..." -ForegroundColor Yellow
    dotnet build --no-incremental

    # Fin de l'analyse
    Write-Host ""
    Write-Host "[3/3] Envoi des resultats a SonarQube..." -ForegroundColor Yellow
    dotnet sonarscanner end `
        /d:sonar.token="$SonarToken"

    Write-Host ""
    Write-Host "==========================================" -ForegroundColor Green
    Write-Host "  Analyse terminee !" -ForegroundColor Green
    Write-Host "  Resultats : $SonarHostUrl/dashboard?id=$SonarProjectKey" -ForegroundColor White
    Write-Host "==========================================" -ForegroundColor Green
} finally {
    Pop-Location
}
