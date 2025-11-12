// See https://aka.ms/new-console-template for more information

using System.Runtime.CompilerServices;
using Ensembles;

Console.WriteLine("Hello, World!");

var list = new List<int>();
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");
list.Add(1);
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");
list.Add(2);
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");
list.Add(3);
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");
list.Add(9);
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");
list.Add(7);
Console.WriteLine($"[A] Ma liste contient actuellement {list.Count} élément(s)");

// Permet de retirer l'élément dont la valeur est 9
list.Remove(9);
Console.WriteLine($"[B] Ma liste contient actuellement {list.Count} élément(s)");

// Permet de retirer l'élément à l'index 0
list.RemoveAt(0);
Console.WriteLine($"[C] Ma liste contient actuellement {list.Count} élément(s)");

// list = new List<int>();
// Permet de supprimer tous les éléments de la liste
list.Clear();
Console.WriteLine($"[D] Ma liste contient actuellement {list.Count} élément(s)");

list = new List<int>() { 1, 2, 3, 9, 7 };
Console.WriteLine($"[E] Ma liste contient actuellement {list.Count} élément(s)");

foreach (var item in list)
{
    Console.WriteLine($"[F] {item}");
}

list.Insert(3, 4);

foreach (var item in list)
{
    Console.WriteLine($"[G] {item}");
}

list = [1, 2, 3, 9, 7];
// List<int> list2 = [];
// var list3 = [];
// var list4 = [1, 2, 3, 9, 7];
foreach (var item in list)
{
    Console.WriteLine($"[H] {item}");
}

// Garbage collector : permet de libérer de la mémoire
// GC.Collect();

if (list.Count >= 1)
    Console.WriteLine($"Mon élément à l'index 0 est {list[0]}");
if (list.Count > 1)
    Console.WriteLine($"Mon élément à l'index 1 est {list[1]}");
if (list.Count >= 3)
    Console.WriteLine($"Mon élément à l'index 2 est {list[2]}");

// Lambda expression or Predicate => 
// Objectif: ne garder que les éléments <= 3 dans la liste 1,2,3,9,7
// => donc retirer tous les éléments > 3
// RemoveAll = LINQ
int removedCount = list.RemoveAll(x => x > 3);
// Removes elements 4, 5, 6
// int removedCount2 = list.RemoveAll(x => x > 3 && x < 7);
// int removedCount2 = list.RemoveAll(x => x is > 3 and < 7);

Console.WriteLine($"{removedCount} élément(s) ont été supprimé(s)");
foreach (var item in list)
{
    Console.WriteLine($"[I] {item}");
}

list = [1, 2, 3, 9, 7];
list.RemoveRange(2, 3);
foreach (var item in list)
{
    Console.WriteLine($"[J] {item}");
}

list = [1, 2, 3, 9, 7];
// list = list.OrderBy(y => y).ToList();
// permet de trier une liste selon pair/impair
list.Sort();
// list = list.OrderBy(y => y % 2).ToList();
foreach (var item in list)
{
    Console.WriteLine($"[K] {item}");
}

list.Reverse();
list = list.OrderByDescending(y => y).ToList();
foreach (var item in list)
{
    Console.WriteLine($"[L] {item}");
}

bool areAllPositive = list.All(x => x > 0);
bool areAnyNegative = list.Any(x => x < 0);

list = [1, 2, 3, 9, 7];
var onlyEven = list
    .Where(x => x % 2 == 0)
    .OrderBy(x => x)
    .ToList();
var onlyOdd = list
    .Where(x => x % 2 != 0)
    .OrderBy(x => x)
    .ToList();
list.Clear();

list = [1, 2, 3, 9, 7];
var myNumber = list.First(x => x == 3);
// returns default value if not found
// List<int> = list with only non-nullable integers
// List<int?> = list with nullable integers
// integer or numeric types = 0
// boolean = 0
// string = ""
var myNumber2 = list.FirstOrDefault(x => x == 15);
Console.WriteLine(myNumber2);

var maxInt = list.Max();
var minInt = list.Min();
var avgInt = list.Average();
var sumInt = list.Sum();

list = [1, 2, 3, 9, 7];
// list.RemoveAt(0);
// list.RemoveRange(4, 1);
var subList = list.Skip(1).Take(3).ToList();
Console.WriteLine($"[L] Ma liste contient {subList.Count} élément(s)");
foreach (var item in subList)
{
    Console.WriteLine($"[M] {item}");
}

var personA = new Person()
{
    FirstName = "John",
    LastName = "Doe",
    NationalNumber = "123456789"
};
var personB = new Person()
{
    FirstName = "Jane",
    LastName = "Doe",
    NationalNumber = "987654321"
};
var personC = new Person()
{
    FirstName = "Georges",
    LastName = "Doe",
    NationalNumber = "66554463"
};
var personList = new List<Person>()
{
    personA,
    personB,
    personC,
};

// personList.GetHashCode();
// personList.Sort();
personList = personList
    .OrderBy(x => x.LastName)
    .ThenBy(x => x.FirstName)
    .ThenBy(x => x.NationalNumber)
    .ToList();

personList = personList
    .Where(x => x.LastName == "Doe")
    .ToList();
    
var dico = new Dictionary<string, Person>();
dico.Add(personA.NationalNumber, personA);
dico.Add(personB.NationalNumber, personB);
dico.Add(personC.NationalNumber, personC);

if (dico.ContainsKey("123456789"))
{
    Person personFromDico = dico["123456789"];
    Console.WriteLine($"Mon élément s'appelle {personFromDico.FirstName}");
}

if (dico.TryGetValue("123456789", out var personFromDico1))
{
    Console.WriteLine($"Mon élément s'appelle {personFromDico1.FirstName}");
}

try
{
    dico.Add(personA.NationalNumber, personA);
}
catch (Exception ex)
{ 
    Console.WriteLine(ex.Message);
}

dico.Remove("123456789");

// HashSet<T> = collection of unique elements
// HashMap<T> = collection of key/value pairs
// SortedSet<T> = collection of unique elements sorted by default
// SortedDictionary<TKey, TValue> = collection of key/value pairs sorted by default