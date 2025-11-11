// Exercice 1 :  Classe Personnage Simple

namespace Exercice_POO_MNeddad
{
    class Program
    {
        static void Main()
        {
            Personnage hero = new Personnage("Iop", "Guerrier", 100, 1);
            hero.Presenter();
            hero.PrendreExp(100);
            hero.Presenter();
            hero.PrendreExp(122);
            hero.Presenter();
        }
    }

    public class Personnage
    {
        public string Nom { get; set; }
        public string Classe { get; set; }
        public int PuntsDeVie { get; set; }
        public int Niveau { get; set; }
        public int Experience;

        public Personnage(string monNom, string maClasse, int mesPoints, int monNiveau)
        {
            Nom = monNom;
            Classe = maClasse;
            PuntsDeVie = mesPoints;
            Niveau = monNiveau;
        }

        public void Presenter()
        {
            Console.WriteLine($"Personnage: {Nom}");
            Console.WriteLine($"Classe : {Classe}");
            Console.WriteLine($"Vie: {PuntsDeVie}");
            Console.WriteLine($"Niveau: {Niveau}");

        }

        public void PrendreExp(int exp)
        {
            if (exp <= 0)
            {
                Console.WriteLine("Aucune expérience gagnée !");
                return;
            }

            Experience += exp;

            // Calcul automatique des niveaux gagnés
            while (Experience >= 100)
            {
                Niveau++;
                Experience -= 100;
                Console.WriteLine($"******** Félicitations ! {Nom} passe au niveau {Niveau} ! ********");
            }

        }

    }
}