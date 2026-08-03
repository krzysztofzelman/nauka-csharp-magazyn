List<Item> magazyn = new List<Item>();
while (true)
{

    Console.WriteLine("📦 Witaj w systemie magazynowym!");
    Console.WriteLine("Wybierz opcje:");
    Console.WriteLine("1 - Dodaj przedmiot");
    Console.WriteLine("2 - Wyświetl wszystkie");
    Console.WriteLine("3 - Edytuj przedmiot");
    Console.WriteLine("4 - Wyjście");
    string wybor = Console.ReadLine() ?? "";

    if (wybor == "1")
    {
        Console.WriteLine("Podaj nazwę przedmiotu:)");
        String nazwa = Console.ReadLine() ?? "";

        Console.WriteLine("Podaj ilość");
        string iloscTekst = Console.ReadLine() ?? "";
        int ilosc;
        while (!int.TryParse(iloscTekst, out ilosc) || ilosc <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj ilość jeszcze raz:");
            iloscTekst = Console.ReadLine () ?? "";
        }

        Console.WriteLine("Podaj cenę za sztukę");
        string cenaTekst = Console.ReadLine() ?? "";
        decimal cena;
        while (!decimal.TryParse(cenaTekst, out cena) || cena <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj cenę jeszcze raz:");
            cenaTekst = Console.ReadLine () ?? "";
        }
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
    else if (wybor == "4")
    {
        break;
    }
    else if (wybor == "3")
    {
        if (magazyn.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie ma czego edytować.");
        }
        else
        {
            Console.WriteLine($"=== Wybierz przedmiot do edycji ===");
            for (int i = 0; i < magazyn.Count; i++)
            {
                Console.WriteLine($"{i + 1} -{magazyn[i].Name} | {magazyn[i].Quantity} szt. |{magazyn[i].Price} zł");
            }
            Console.WriteLine("Wybierz numer przedmiotu do edycji: ");
            string wyborEdycji = Console.ReadLine () ?? "";
            int indeks;
            if (!int.TryParse(wyborEdycji, out indeks) || indeks < 1|| indeks > magazyn.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else

            {
                indeks --;
                 Item edytowany = magazyn[indeks];
                Console.WriteLine($"Podaj nową nazwę (Enter = zostaw {edytowany.Name}):");
                string nowaNazwa = Console.ReadLine() ?? "";
                if (nowaNazwa != "")
                {
                    edytowany.Name = nowaNazwa;
                }

                Console.WriteLine($"Podaj nową ilość (Enter = zostaw {edytowany.Quantity}):");
                string nowaIloscTekst = Console.ReadLine() ?? "";
                if (nowaIloscTekst != "")
                {
                    int nowaIlosc;
                    while (!int.TryParse(nowaIloscTekst, out nowaIlosc) || nowaIlosc <= 0)
                    {
                        Console.WriteLine("❌ Nieprawidłowa liczba! Podaj ilość jeszcze raz:");
                        nowaIloscTekst = Console.ReadLine() ?? "";
                    }
                    edytowany.Quantity = nowaIlosc;
                }

                Console.WriteLine($"Podaj nową cenę (Enter = zostaw {edytowany.Price}):");
                string nowaCenaTekst = Console.ReadLine() ?? "";
                if (nowaCenaTekst != "")
                {
                    decimal nowaCena;
                    while (!decimal.TryParse(nowaCenaTekst, out nowaCena) || nowaCena <= 0)
                    {
                        Console.WriteLine("❌ Nieprawidłowa liczba! Podaj cenę jeszcze raz:");
                        nowaCenaTekst = Console.ReadLine() ?? "";
                    }
                    edytowany.Price = nowaCena;
                }
            }
        }
    }
    else
    {
        Console.WriteLine("❌ Nie ma takiej opcji!");
    }

}