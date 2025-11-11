namespace Exercice_POO_MNeddad;

public class ValidateurNrn
{
    public static bool Valider(string nrn)
    {
        if (nrn.Length != 11)
        {
            Console.WriteLine("Le numero nationale doit contenir 11 chiffres");
            return false;
        }
        if (!nrn.All(char.IsDigit))
        {
            Console.WriteLine("Le numero nationale doit contenir que des chiffres");
            return false;
        }

        int.TryParse(nrn[2..4], out int mois);
        int.TryParse(nrn[4..6], out int jour);
        int.TryParse(nrn[..9], out int nrnSansCc);
        int.TryParse(nrn[9..11], out int checksum);

        int calculatedChecksum = 97 - (nrnSansCc % 97);

        if (mois is < 1 or > 12)
        {
            Console.WriteLine("Le mois doit être entre 1 et 12");
            return false;
        }
        
        if (jour is < 1 or > 31)
        {
            Console.WriteLine("Le jour doit être entre 1 et 31");
            return false;
        }
            
        if (calculatedChecksum != checksum)
        {
            Console.WriteLine($"Le checksum est invalide : calculé = {calculatedChecksum}, fourni = {checksum}");
            return false;
        }
        Console.WriteLine("Le numero de registre nationale fourni est Valide");
        return true;
    }

}