public class Etudiant
{
    public string Nom { get; set; }
    public string Matricule { get; set; }
    public List<decimal> Notes = new List<decimal>();
    
    public Etudiant(string nom, string matricule, List<decimal> notes)
    {
        Nom = nom;
        Matricule = matricule;
        Notes = notes;
    }

    public void AjouterNote(decimal note)
    {
        if (note < 0 || note > 20)
        {
            Console.WriteLine("Note invalide: la note doit être entre 0 et 20");
            return;
        }
        Notes.Add(note);
        Console.WriteLine($"Note {note} ajoutée à {Nom}");
    }

    public decimal CalculerMoyenne()
    {
        if (Notes.Count == 0)
        {
            Console.WriteLine($"{Nom} n’a pas encore de notes.");
            return 0;
        }
        return Notes.Average();
    }

    public bool Reussi()
    {
        return CalculerMoyenne() >= 12;
    }

    public void Afficher()
    {
        Console.WriteLine($"L'etudiant {Nom} : {Matricule} a {(Reussi()? "Reussi" : "Echoue")} avec une moyenne general : {CalculerMoyenne()}/20");
    }
}

public class Classe
{
    public string Nom { get; set; }
    public List<Etudiant> Etudiants { get; set; }

    public Classe(string nom, List<Etudiant> etudiants)
    {
        Nom = nom;
        Etudiants = etudiants;
    }

    public void AjouterEtudiant(Etudiant e)
    {
        Etudiants.Add(e);
    }

    public decimal MoyenneClasse()
    {
        if (Etudiants.Count == 0)
        {
            Console.WriteLine($"La classe {Nom} n’a pas encore d'etudiant.");
            return 0;
        }
        decimal sommeNotesClasse = 0;
        foreach (Etudiant etudiant in Etudiants)
        {
            sommeNotesClasse += etudiant.CalculerMoyenne(); 
        }
        decimal moyenneClass = sommeNotesClasse/Etudiants.Count;
        return moyenneClass;
    }

    public Etudiant? MeilleurEtudiant()
    {
        if (Etudiants.Count == 0) return null;
        
        Etudiant meilleurEtudiant = Etudiants[0];
        decimal meilleureMoyenne = meilleurEtudiant.CalculerMoyenne();
        foreach (Etudiant etudiant  in Etudiants)
        {
            decimal moyenne = etudiant.CalculerMoyenne();
            if (moyenne > meilleureMoyenne)
            {
                meilleureMoyenne = moyenne;
                meilleurEtudiant = etudiant;
            }
        }
        return meilleurEtudiant;
    }

    public int NbReussis()
    {
        int nbReussis = 0;
        foreach (Etudiant etudiant  in Etudiants)
        {
            if (etudiant.Reussi())
            {
                nbReussis++;
            }
        }
        return nbReussis;
    }

    public void AfficherTous()
    {
        if (Etudiants.Count == 0)
        {
            Console.WriteLine($"Aucun étudiant dans la classe {Nom}.");
            return;
        }
        Console.WriteLine($" La liste des notes des etudiants de la Classe : {Nom}");
        foreach (Etudiant etudiant in Etudiants)
        {
            etudiant.Afficher();
        }

        Console.WriteLine($"La moyenne de la classe est {MoyenneClasse()}");
        Console.WriteLine($"Le nombere des etudiants reussis est {NbReussis()}/{Etudiants.Count}");
        Console.WriteLine($"Le meilleur étudiant est {MeilleurEtudiant().Nom}");
        
    }
}