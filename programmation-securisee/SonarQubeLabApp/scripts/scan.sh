#!/bin/bash
# ==============================================
# SonarQube Lab - Script d'analyse .NET
# ==============================================

set -e

# Configuration
SONAR_HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
SONAR_PROJECT_KEY="${SONAR_PROJECT_KEY:-my-dotnet-project}"
SONAR_TOKEN="${SONAR_TOKEN:-}"
PROJECT_PATH="${1:-.}"

echo "=========================================="
echo "  SonarQube - Analyse .NET"
echo "=========================================="

# Verification du token
if [[ -z "$SONAR_TOKEN" ]]; then
    echo "[ERREUR] SONAR_TOKEN n'est pas defini."
    echo ""
    echo "Generez un token dans SonarQube :"
    echo "  1. Allez sur $SONAR_HOST_URL"
    echo "  2. Mon Compte > Securite > Generer un token"
    echo "  3. Exportez : export SONAR_TOKEN=sqp_votre_token"
    exit 1
fi

# Verification de dotnet
if ! command -v dotnet &> /dev/null; then
    echo "[ERREUR] .NET SDK n'est pas installe."
    exit 1
fi

# Verification/Installation du scanner .NET
if ! dotnet tool list -g | grep -q "dotnet-sonarscanner"; then
    echo "[INFO] Installation de dotnet-sonarscanner..."
    dotnet tool install --global dotnet-sonarscanner
fi

echo "[INFO] Analyse du projet : $PROJECT_PATH"
echo "[INFO] Cle du projet    : $SONAR_PROJECT_KEY"
echo "[INFO] Serveur SonarQube: $SONAR_HOST_URL"
echo ""

cd "$PROJECT_PATH"

# Debut de l'analyse
echo "[1/3] Demarrage de l'analyse SonarQube..."
dotnet sonarscanner begin \
    /k:"$SONAR_PROJECT_KEY" \
    /d:sonar.host.url="$SONAR_HOST_URL" \
    /d:sonar.token="$SONAR_TOKEN"

# Build du projet
echo ""
echo "[2/3] Build du projet .NET..."
dotnet build --no-incremental

# Fin de l'analyse et envoi des resultats
echo ""
echo "[3/3] Envoi des resultats a SonarQube..."
dotnet sonarscanner end \
    /d:sonar.token="$SONAR_TOKEN"

echo ""
echo "=========================================="
echo "  Analyse terminee !"
echo "  Resultats : $SONAR_HOST_URL/dashboard?id=$SONAR_PROJECT_KEY"
echo "=========================================="
