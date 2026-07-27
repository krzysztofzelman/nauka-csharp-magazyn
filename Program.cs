List<Item> magazyn = new List<Item>();
while (true)
{

    Console.WriteLine("📦 Witaj w systemie magazynowym!");
    Console.WriteLine("Wybierz opcje:");
    Console.WriteLine("1 - Dodaj przedmiot");
    Console.WriteLine("2 - Wyświetl wszystkie");
    Console.WriteLine("3 - Wyjście");
    string wybor = Console.ReadLine() ?? "";

    if (wybor == "1")
    {
        Console.WriteLine("Podaj nazwę przedmiotu:)");
        String nazwa = Console.ReadLine() ?? "";

        Console.WriteLine("Podaj ilość");
        string iloscTekst = Console.ReadLine() ?? "";
        int ilosc = int.Parse(iloscTekst);

        Console.WriteLine("Podaj cenę za sztukę");
        string cenaTekst = Console.ReadLine() ?? "";
        decimal cena = decimal.Parse(cenaTekst);
        magazyn.Add(new Item(nazwa,ilosc,cena));
    }
    else if (wybor == "2")
        if (magazyn.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty.");
        }
        else
        {
            Console.WriteLine("=== Zawartość magazynu ===");
            foreach (Item przedmiot in magazyn)
            {
                Console.WriteLine($"-{przedmiot.Name} | {przedmiot.Quantity} szt. | {przedmiot.Price} zł");
            }
        }
    else if (wybor == "3")
    {
        break;
    }
    else
    {
        Console.WriteLine("❌ Nie ma takiej opcji!");
    }

}