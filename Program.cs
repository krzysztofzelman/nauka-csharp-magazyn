using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;

string connectionString = "Data Source=magazyn.db";
using (SqliteConnection connection = new SqliteConnection(connectionString))
{
    connection.Open();
    SqliteCommand command = connection.CreateCommand();
    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Przedmioty (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        Nazwa TEXT NOT NULL,
        Ilosc INTEGER NOT NULL,
        Cena REAL NOT NULL
        )";
    command.ExecuteNonQuery();
}

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

    if (wybor == "1")       // Dodawanie przedmiotu
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

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Przedmioty (Nazwa, Ilosc, Cena) VALUES (@nazwa, @ilosc, @cena)";
            command.Parameters.AddWithValue("@nazwa", nazwa);
            command.Parameters.AddWithValue("@ilosc", ilosc);
            command.Parameters.AddWithValue("@cena", cena);
            command.ExecuteNonQuery();
        }
    }
    else if (wybor == "2")      // Wyświetlanie wszystkich przedmiotów
    {
       using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Nazwa, Ilosc, Cena FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            Console.WriteLine("==== Zawartość magazynu ====");
            bool pokazanoCos = false;
            while (reader.Read())
            {
                pokazanoCos = true;
                Console.WriteLine($"- {reader["Nazwa"]} | Ilość: {reader["Ilosc"]} | Cena: {reader["Cena"]} zł");
            }
            if (!pokazanoCos)
            {
                Console.WriteLine("📭 Magazyn jest pusty.");
            }
        }
    }
    else if (wybor == "3")      // Edytowanie przedmiotu
    {
        List<int> idy = new List<int>();
        using (SqliteConnection  connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nazwa, Ilosc, Cena FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            int numer = 1;
            Console.WriteLine("=== Wybierz przedmiot do edycji ===");
            while (reader.Read())
            {
                idy.Add(Convert.ToInt32(reader["Id"]));
                Console.WriteLine($"{numer}. {reader["Nazwa"]} | {reader["Ilosc"]} szt. | {reader["Cena"]} zł");
                numer++;
            }
        }
        if (idy.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie ma czego edytować.");
        }
        else
        {
            Console.WriteLine("Podaj numer przedmiotu do edycji:");
            string wyborEdycji = Console.ReadLine() ?? "";
            int indeks;
            if (!int.TryParse(wyborEdycji, out indeks) || indeks < 1 || indeks > idy.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else
            {
                indeks--;
                int wybraneId = idy[indeks];

                Item edytowany;
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT Nazwa, Ilosc, Cena FROM Przedmioty WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", wybraneId);
                    SqliteDataReader reader = command.ExecuteReader();
                    reader.Read();
                    edytowany = new Item(
                        Convert.ToString(reader["Nazwa"]) ?? "",
                        Convert.ToInt32(reader["Ilosc"]),
                        Convert.ToDecimal(reader["Cena"]));
                }

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

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Przedmioty SET Nazwa = @nazwa, Ilosc = @ilosc, Cena = @cena WHERE Id = @id";
                    command.Parameters.AddWithValue("@nazwa", edytowany.Name);
                    command.Parameters.AddWithValue("@ilosc", edytowany.Quantity);
                    command.Parameters.AddWithValue("@cena", edytowany.Price);
                    command.Parameters.AddWithValue("@id", wybraneId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    else if (wybor == "4")      // Usuwanie przedmiotu
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();  
            command.CommandText = "SELECT Id, Nazwa, Ilosc, Cena FROM Przedmioty"; 
            SqliteDataReader reader = command.ExecuteReader();  
            List<int> idy = new List<int>();    
            List<string> nazwy = new List<string>();    
            int numer = 1;  
            Console.WriteLine("=== Wybierz przedmiot do usunięcia ===");    
            while (reader.Read()) 
            {
                idy.Add(Convert.ToInt32(reader["Id"])); 
                nazwy.Add(Convert.ToString(reader["Nazwa"]) ?? ""); 
                Console.WriteLine($"{numer}. {reader["Nazwa"]} | {reader["Ilosc"]} szt. | {reader["Cena"]} zł" 
                    ); 
                numer++;
            }
            if (idy.Count == 0)
            {
                Console.WriteLine("📭 Magazyn jest pusty, nie ma czego usuwać.");

            }
            else 
            {
                Console.WriteLine("Podaj numer przedmiotu do usunięcia:"); 
                string wyborUsuniecia = Console.ReadLine() ?? ""; 
                int indeks; 
                if (!int.TryParse(wyborUsuniecia, out indeks) || indeks < 1 || indeks > idy.Count)
                {
                    Console.WriteLine("❌ Nieprawidłowy numer!");
                }
                else 
                {
                    indeks--; 
                    int id = idy[indeks];
                    SqliteCommand komendaUsun = connection.CreateCommand();
                    komendaUsun.CommandText = "DELETE FROM Przedmioty WHERE Id = @id";
                    komendaUsun.Parameters.AddWithValue("@id", id);
                    komendaUsun.ExecuteNonQuery();
                    Console.WriteLine($"✅ Usunięto przedmiot: {nazwy[indeks]}");
                    
                }
            }
        }
    }

    else if (wybor == "5")      // Obliczanie wartości magazynu
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT SUM(Ilosc * Cena) FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            reader.Read();
            if (reader.IsDBNull(0))
            {
                Console.WriteLine("📭 Magazyn jest pusty, wartość wynosi 0 zł.");
            }
            else
            {
                decimal suma = Convert.ToDecimal(reader[0]);
                Console.WriteLine($"💰 Wartość magazynu: {suma} zł");
            }
        }
    }
    else if (wybor == "6")      // Wyjście z programu
    {
        break;
    }
    else
    {
        Console.WriteLine("❌ Nie ma takiej opcji!");
    }
}
