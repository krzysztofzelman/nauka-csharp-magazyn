List<Item> magazyn = new List<Item>();

while (true)
{
    Console.WriteLine("📦 Witaj w systemie magazynowym!");
    Console.WriteLine("Wybierz opcje:");
    Console.WriteLine("1 - Dodaj przedmiot");
    Console.WriteLine("2 - Wyświetl wszystkie");
    Console.WriteLine("3 - Edytuj przedmiot");
    Console.WriteLine("4 - Usuń przedmiot");
    Console.WriteLine("5 - Wartość magazynu");
    Console.WriteLine("6 - Wyjście");
    string wybor = Console.ReadLine() ?? "";

    if (wybor == "1")
    {
        Console.WriteLine("Podaj nazwę przedmiotu:)");
        string nazwa = Console.ReadLine() ?? "";

        Console.WriteLine("Podaj ilość");
        string iloscTekst = Console.ReadLine() ?? "";
        int ilosc;
        while (!int.TryParse(iloscTekst, out ilosc) || ilosc <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj ilość jeszcze raz:");
            iloscTekst = Console.ReadLine() ?? "";
        }

        Console.WriteLine("Podaj cenę za sztukę");
        string cenaTekst = Console.ReadLine() ?? "";
        decimal cena;
        while (!decimal.TryParse(cenaTekst, out cena) || cena <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj cenę jeszcze raz:");
            cenaTekst = Console.ReadLine() ?? "";
        }
        magazyn.Add(new Item(nazwa, ilosc, cena));
    }
    else if (wybor == "2")
    {
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
    }
    else if (wybor == "3")
    {
        if (magazyn.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie ma czego edytować.");
        }
        else
        {
            Console.WriteLine("=== Wybierz przedmiot do edycji ===");
            Funkcje.WyswietlListe(magazyn);
            Console.WriteLine("Wybierz numer przedmiotu do edycji: ");
            string wyborEdycji = Console.ReadLine() ?? "";
            int indeks;
            if (!int.TryParse(wyborEdycji, out indeks) || indeks < 1 || indeks > magazyn.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else
            {
                indeks--;
                Item edytowany = magazyn[indeks];
                Console.WriteLine("Podaj nową nazwę (popraw i wciśnij Enter):");
                Console.Write(edytowany.Name);
                string nowaNazwa = edytowany.Name;
                while (true)
                {
                    ConsoleKeyInfo klawisz = Console.ReadKey(true);
                    if (klawisz.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else if (klawisz.Key == ConsoleKey.Backspace && nowaNazwa.Length > 0)
                    {
                        nowaNazwa = nowaNazwa.Substring(0, nowaNazwa.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (klawisz.KeyChar != '\0')
                    {
                        nowaNazwa += klawisz.KeyChar;
                        Console.Write(klawisz.KeyChar);
                    }
                }
                Console.WriteLine();
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
    else if (wybor == "4")
    {
        if (magazyn.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie ma czego usuwać.");
        }
        else
        {
            Console.WriteLine("=== Wybierz przedmiot do usunięcia ===");
            Funkcje.WyswietlListe(magazyn);
            Console.WriteLine("Wybierz numer przedmiotu do usunięcia: ");
            string wyborUsuniecia = Console.ReadLine() ?? "";
            int indeks;
            if (!int.TryParse(wyborUsuniecia, out indeks) || indeks < 1 || indeks > magazyn.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else
            {
                indeks--;
                Item usuwany = magazyn[indeks];
                magazyn.RemoveAt(indeks);
                Console.WriteLine($"✅ Usunięto przedmiot: {usuwany.Name}");
            }
        }
    }

    else if (wybor == "5")
    {
        decimal wartosc = Funkcje.WartoscMagazynu(magazyn);
        Console.WriteLine($"💰 Wartość magazynu: {wartosc} zł");
    }
    else if (wybor == "6")
    {
        break;
    }
    else
    {
        Console.WriteLine("❌ Nie ma takiej opcji!");
    }
}
