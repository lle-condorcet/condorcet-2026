using Exercice_POO_MNeddad;
using static System.Console;

WriteLine("-------- Exercice 1 --------");
Personnage hero = new Personnage("Iop", "Guerrier", 100, 1);
hero.Presenter();
hero.PrendreExp(100);
hero.Presenter();
hero.PrendreExp(122);
hero.Presenter();

WriteLine("-------- Exercice 2 --------");
CompteBancaire compteDries = new CompteBancaire("Dries", "BE12 1111 2222 3333", 20);
compteDries.AfficherSolde();
compteDries.Deposer(100);
compteDries.Retirer(64);
compteDries.Deposer(872);
compteDries.Retirer(3000);
compteDries.AfficherSolde();

CompteBancaire compteSofie = new CompteBancaire("Sophie", "BE12 1111 2222 3333", -1203);
compteSofie.AfficherSolde();
try
{
    compteSofie.Solde = 13000; 
}
catch (Exception e)
{
    WriteLine($"error : {e.Message}");
}
WriteLine($"Le solde de {compteSofie.Titulaire} est : {compteSofie.Solde} Eur");


WriteLine("-------- Exercice 3 --------");

// Liste de NRN à tester avec une couverture de 100%
string[] nrnTests =
{
    "98092945520",  // Valide
    "98132945520",  // Mois 13
    "98093245520",  // Jour 32
    "98092945529",  // Mauvais checksum
    "9809294552",   // Trop court
    "980929455201", // Trop long
    "98G92945520",  // Contient une lettre
    "00010100173",  // Valide (1er janvier 2000)
    "99123100168",  // Valide (31 décembre 1999)
    ""              // Vide
};

// Boucle de test
foreach (var nrn in nrnTests)
{
    WriteLine($"\nTest du NRN : {nrn}");
    ValidateurNrn.Valider(nrn);
}

WriteLine("-------- Exercice 4 --------");

Magasin magasin = new Magasin("EspaceMarket");
Produit banane = new Produit("Banane", 1.49m, 150);
Produit lait = new Produit("Lait", 0.92m, 97);

magasin.AjouterProduit(banane);
magasin.AjouterProduit(lait);
magasin.AfficherProduits();

Produit selectBanane = magasin.TrouverProduit("Banane");
decimal prixBanane = selectBanane.Acheter(2);
if (prixBanane>0) WriteLine($"Total a payer : {prixBanane} Eur");
magasin.AfficherProduits();
Produit selectPomme = magasin.TrouverProduit("Pomme");
Produit selectLait = magasin.TrouverProduit("Lait");
decimal prixLait1 = selectLait.Acheter(-2);
if (prixLait1>0) WriteLine($"Total a payer : {prixLait1} Eur");
decimal prixLait2 = selectLait.Acheter(120);
if (prixLait2>0) WriteLine($"Total a payer : {prixLait2} Eur");