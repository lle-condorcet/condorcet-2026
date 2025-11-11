namespace Exercice_POO_MNeddad;

public class CompteBancaire
{
    public string Titulaire { get; set; }
    public string NumeroCompte { get; set; }
    private decimal _solde;

    public decimal Solde {
        get { return _solde; }
        set { throw new InvalidOperationException("Le solde ne peut pas être modifié directement !"); }
    }
    
    public CompteBancaire(string titulaire, string numeroCompte, decimal soldeInit)
    {
        Titulaire = titulaire;
        NumeroCompte = numeroCompte;
        _solde = soldeInit >= 0 ? soldeInit : 0;
    }

    public void Deposer(decimal montant)
    {
        if (montant > 0)
        {
            _solde += montant;
            Console.WriteLine($"Un dépôt de {montant}EUR a été effectué");
        }
        else
        {
            Console.WriteLine("Montant invalide");
        }
    }

    public void Retirer(decimal montant)
    {
        if (montant <= 0)
        {
            Console.WriteLine("Montant invalide !");
            return;
        }
        if (_solde >= montant)
        {
            _solde -= montant;
            Console.WriteLine($"Un retrait de {montant}Eur a été effectué");
        }
        else
        {
            Console.WriteLine("Solde insuffisant pour effectuer cette operation");
        }
    }

    public void AfficherSolde()
    {
        Console.WriteLine($"Le nouveau solde du compte {NumeroCompte} - {Titulaire} est: {_solde}Eur");
    }
}
