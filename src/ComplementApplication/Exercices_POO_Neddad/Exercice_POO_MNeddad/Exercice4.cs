namespace Exercice_POO_MNeddad;

public class Produit
{
    public string Nom  { get; set; }
    public decimal Prix  { get; set; }
    public int Stock  { get; set; }

    public Produit(string nom, decimal prix, int stock)
    {
        Nom = nom;
        Prix = prix;
        Stock = stock;
    }

    public void AjouterStock(int quantite)
    {
        if (quantite > 0)
        {
            Stock  += quantite;
            Console.WriteLine($"Le nouveau stock du produit {Nom} est : {Stock}");
            return;
        }
        Console.WriteLine("Quantité invalide");
    }
    
    public void RetirerStock(int quantite)
    {
        if (quantite <= Stock && quantite > 0)
        {
            Stock  -= quantite;
            Console.WriteLine($"Le nouveau stock du produit {Nom} est : {Stock}");
        }
    }
    public Decimal Acheter(int quantite)
    {
        if (quantite <= Stock && quantite > 0)
        {
            RetirerStock(quantite);
            Console.WriteLine($"{quantite} unité(s) du produit {Nom} achetée(s). Stock restant : {Stock}");
            return Prix*quantite;
        }
        Console.WriteLine("Achat impossible : quantité demandée invalide ou stock insuffisant");
        return -1;

    }
    
}

public class Magasin
{
    public string Nom { get; set; }
    public List<Produit> Produits= new List<Produit>();

    public Magasin(string nom)
    {
        Nom = nom;
    }

    public void AjouterProduit(Produit produit)
    {
        Produits.Add(produit);
        Console.WriteLine($"Le produit {produit.Nom} est ajouté au magasin {Nom}");
    }

    public void AfficherProduits()
    {
        if (Produits.Count == 0)
        {
            Console.WriteLine("Aucun produit dans le magasin.");
            return;
        }
        Console.WriteLine($"La liste des produits disponibles dans le magasin {Nom} :");
        foreach (Produit produit in Produits)
        {
            Console.WriteLine($"Nom du produit: {produit.Nom}");
            Console.WriteLine($"Prix: {produit.Prix} Eur");
            Console.WriteLine($"Stock: {produit.Stock}");
            
        }
    }

    public Produit TrouverProduit(string nom)
    {
        foreach (Produit produit in Produits)
        {
            if (produit.Nom == nom)
            {
                return produit;
            }
        }

        return null;
    }
}