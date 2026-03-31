#!/bin/bash
# ==============================================
# SonarQube Lab - Script d'installation
# ==============================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"

echo "=========================================="
echo "  SonarQube Lab - Installation"
echo "=========================================="

# Verification de Docker
if ! command -v docker &> /dev/null; then
    echo "[ERREUR] Docker n'est pas installe."
    echo "Installez Docker Desktop : https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Verification de Docker Compose
if ! docker compose version &> /dev/null; then
    echo "[ERREUR] Docker Compose n'est pas disponible."
    exit 1
fi

echo "[OK] Docker et Docker Compose detectes."

# Configuration vm.max_map_count pour Linux (requis par Elasticsearch dans SonarQube)
if [[ "$(uname)" == "Linux" ]]; then
    CURRENT_MAP_COUNT=$(sysctl -n vm.max_map_count 2>/dev/null || echo "0")
    if [[ "$CURRENT_MAP_COUNT" -lt 262144 ]]; then
        echo "[INFO] Configuration de vm.max_map_count (requis par Elasticsearch)..."
        sudo sysctl -w vm.max_map_count=262144
        echo "vm.max_map_count=262144" | sudo tee -a /etc/sysctl.conf > /dev/null
        echo "[OK] vm.max_map_count configure."
    else
        echo "[OK] vm.max_map_count deja configure ($CURRENT_MAP_COUNT)."
    fi
fi

# Lancement des conteneurs
echo ""
echo "[INFO] Demarrage de SonarQube et PostgreSQL..."
cd "$PROJECT_DIR"
docker compose up -d

echo ""
echo "[INFO] Attente du demarrage de SonarQube (peut prendre 1-2 minutes)..."
echo "       Suivez les logs avec : docker compose logs -f sonarqube"

# Attente que SonarQube soit pret
MAX_WAIT=180
ELAPSED=0
while [[ $ELAPSED -lt $MAX_WAIT ]]; do
    STATUS=$(curl -s http://localhost:9000/api/system/status 2>/dev/null | grep -o '"status":"[^"]*"' | cut -d'"' -f4 || echo "")
    if [[ "$STATUS" == "UP" ]]; then
        echo ""
        echo "=========================================="
        echo "  SonarQube est pret !"
        echo "=========================================="
        echo ""
        echo "  URL     : http://localhost:9000"
        echo "  Login   : admin"
        echo "  Password: admin"
        echo ""
        echo "  IMPORTANT: Changez le mot de passe admin"
        echo "  au premier login !"
        echo "=========================================="
        exit 0
    fi
    sleep 5
    ELAPSED=$((ELAPSED + 5))
    echo "  ...en attente ($ELAPSED/${MAX_WAIT}s)"
done

echo ""
echo "[ATTENTION] SonarQube n'est pas encore pret apres ${MAX_WAIT}s."
echo "Verifiez les logs : docker compose logs sonarqube"
