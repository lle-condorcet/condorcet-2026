// cors-demo.js — Fonctions partagees pour les demonstrations CORS

const CORS_API_BASE = 'http://localhost:5300';

// Verification du statut d'authentification au chargement de la page
document.addEventListener('DOMContentLoaded', async () => {
    const statusEl = document.getElementById('auth-status');
    if (!statusEl) return;

    try {
        const response = await fetch(`${CORS_API_BASE}/api/auth/status`, {
            credentials: 'include'
        });
        const data = await response.json();
        if (data.authenticated) {
            statusEl.innerHTML = `<span class="badge bg-success">Connecte : ${data.user}</span>`;
        } else {
            statusEl.innerHTML = `<span class="badge bg-secondary">Non connecte</span>`;
        }
    } catch {
        statusEl.innerHTML = `<span class="badge bg-danger">API hors ligne</span>`;
    }
});
