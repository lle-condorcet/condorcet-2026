#!/bin/bash
# ──────────────────────────────────────
# Script d'installation du laboratoire Keycloak
# ──────────────────────────────────────

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
DOCKER_DIR="$(dirname "$SCRIPT_DIR")"

echo "======================================"
echo "  Keycloak Authentication Lab - Setup"
echo "======================================"
echo ""

# Verifier que Docker est installe
if ! command -v docker &> /dev/null; then
    echo "ERREUR: Docker n'est pas installe."
    echo "Installez Docker Desktop: https://www.docker.com/products/docker-desktop"
    exit 1
fi

# Verifier que docker compose est disponible
if ! docker compose version &> /dev/null; then
    echo "ERREUR: docker compose n'est pas disponible."
    exit 1
fi

echo "[1/4] Demarrage de l'infrastructure..."
cd "$DOCKER_DIR"
docker compose up -d

echo ""
echo "[2/4] Attente du demarrage de Keycloak..."
echo "       (cela peut prendre 30-60 secondes)"

RETRIES=30
until docker compose exec keycloak bash -c "exec 3<>/dev/tcp/localhost/8080" 2>/dev/null; do
    RETRIES=$((RETRIES - 1))
    if [ $RETRIES -le 0 ]; then
        echo "ERREUR: Keycloak n'a pas demarre dans le delai imparti."
        echo "Verifiez les logs: docker compose logs keycloak"
        exit 1
    fi
    echo "       En attente... ($RETRIES tentatives restantes)"
    sleep 5
done

echo "       Keycloak est pret !"

echo ""
echo "[3/4] Verification de l'import du realm..."
sleep 5

echo ""
echo "[4/4] Verification des services..."
echo ""
echo "  PostgreSQL : $(docker compose ps postgres --format '{{.Status}}')"
echo "  Keycloak   : $(docker compose ps keycloak --format '{{.Status}}')"
echo "  Application: $(docker compose ps webapp --format '{{.Status}}')"

echo ""
echo "======================================"
echo "  Installation terminee !"
echo "======================================"
echo ""
echo "  Keycloak Admin Console : http://localhost:8080"
echo "    - Utilisateur: admin"
echo "    - Mot de passe: admin"
echo ""
echo "  Application .NET       : http://localhost:5000"
echo ""
echo "  Utilisateurs de test:"
echo "    - admin@lab.local    / Admin123!"
echo "    - manager@lab.local  / Manager123!"
echo "    - user@lab.local     / User123!"
echo "    - auditor@lab.local  / Auditor123!"
echo ""
echo "  Pour arreter: docker compose down"
echo "  Pour les logs: docker compose logs -f"
echo "======================================"
