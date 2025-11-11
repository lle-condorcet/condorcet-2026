using Exercice_POO_MNeddad;

Console.WriteLine("-------- Exercice 1 --------");
Personnage hero = new Personnage("Iop", "Guerrier", 100, 1);
hero.Presenter();
hero.PrendreExp(100);
hero.Presenter();
hero.PrendreExp(122);
hero.Presenter();

Console.WriteLine("-------- Exercice 2 --------");
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
    Console.WriteLine($"error : {e.Message}");
}
Console.WriteLine($"Le solde de {compteSofie.Titulaire} est : {compteSofie.Solde} Eur");


