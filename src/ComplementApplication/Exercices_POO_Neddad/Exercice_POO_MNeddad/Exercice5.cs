public class Animal
{
    public string Nom { get; set; }
    public int Age { get; set; }
    public string EspeceType { get; set; }

    public Animal(string nom, int age, string especeType)
    {
        Nom = nom;
        Age = age;
        EspeceType = especeType;
    }

    public virtual void FaireBruit()
    {
        Console.WriteLine($"{Nom} fait un bruit");
    }

    public void Manger(string nourriture)
    {
        Console.WriteLine($"{Nom} mange {nourriture}");
        
    }
}

public class Chien : Animal
{
    public Chien(string nom, int age, string especeType) : base(nom, age, especeType){}
    public override void FaireBruit()
    {
        Console.WriteLine("Woof!");
    }
}
public class Chat : Animal
{
    public Chat(string nom, int age, string especeType) : base(nom, age, especeType){}
    public override void FaireBruit()
    {
        Console.WriteLine("Miaou!");
    }
}
public class Oiseau  : Animal
{
    public string Couleur { get; set; }

    public Oiseau(string nom, int age, string especeType, string couleur) : base(nom, age, especeType)
    {
        Couleur = couleur;
    }
    public override void FaireBruit()
    {
        Console.WriteLine("Piou piou!");
    }
}