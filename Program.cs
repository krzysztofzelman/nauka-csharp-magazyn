using System.Runtime.InteropServices;
using Microsoft.Data.Sqlite;

string connectionString = "Data Source=D:\\Dane\\Projekty\\NaukaCSharp\\MagazynBaza\\magazyn.db";
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

using (SqliteConnection connection = new SqliteConnection(connectionString))
{
    connection.Open();
    SqliteCommand command = connection.CreateCommand();
    command.CommandText = @"
        CREATE TABLE IF NOT EXISTS Partie (
        Id INTEGER PRIMARY KEY AUTOINCREMENT,
        PrzedmiotId INTEGER,
        Ilosc INTEGER,
        Cena REAL,
        Data TEXT,
        Status TEXT,
        BatchNumber TEXT
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
    Console.WriteLine("6 - PZ (przyjęcie dostawy)");
    Console.WriteLine("7 - WZ (wydanie towaru)");
    Console.WriteLine("8 - Wyjście");
    string choice = (Console.ReadLine() ?? "").Trim();

    if (choice == "1")       // Dodawanie przedmiotu
    {
        Console.WriteLine("Podaj nazwę przedmiotu:)");
        string name = Console.ReadLine() ?? "";

        Console.WriteLine("Podaj ilość");
        string quantityText = Console.ReadLine() ?? "";
        int quantity;
        while (!int.TryParse(quantityText, out quantity) || quantity <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj ilość jeszcze raz:");
            quantityText = Console.ReadLine() ?? "";
        }

        Console.WriteLine("Podaj cenę za sztukę");
        string priceText = Console.ReadLine() ?? "";
        decimal price;
        while (!decimal.TryParse(priceText, out price) || price <= 0)
        {
            Console.WriteLine("❌ Nieprawidłowa liczba! podaj cenę jeszcze raz:");
            priceText = Console.ReadLine() ?? "";
        }

        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "INSERT INTO Przedmioty (Nazwa, Ilosc, Cena) VALUES (@name, @quantity, @price)";
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@quantity", quantity);
            command.Parameters.AddWithValue("@price", price);
            command.ExecuteNonQuery();
        }
    }
    else if (choice == "2")      // Wyświetlanie wszystkich przedmiotów
    {
       using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Nazwa, Ilosc, Cena FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            Console.WriteLine("==== Zawartość magazynu ====");
            bool anythingShown = false;
            while (reader.Read())
            {
                anythingShown = true;
                Console.WriteLine($"- {reader["Nazwa"]} | Ilość: {reader["Ilosc"]} | Cena: {reader["Cena"]} zł");
            }
            if (!anythingShown)
            {
                Console.WriteLine("📭 Magazyn jest pusty.");
            }
        }
    }
    else if (choice == "3")      // Edytowanie przedmiotu
    {
        List<int> ids = new List<int>();
        using (SqliteConnection  connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nazwa, Ilosc, Cena FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            int number = 1;
            Console.WriteLine("=== Wybierz przedmiot do edycji ===");
            while (reader.Read())
            {
                ids.Add(Convert.ToInt32(reader["Id"]));
                Console.WriteLine($"{number}. {reader["Nazwa"]} | {reader["Ilosc"]} szt. | {reader["Cena"]} zł");
                number++;
            }
        }
        if (ids.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie ma czego edytować.");
        }
        else
        {
            Console.WriteLine("Podaj numer przedmiotu do edycji:");
            string editChoice = Console.ReadLine() ?? "";
            int index;
            if (!int.TryParse(editChoice, out index) || index < 1 || index > ids.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else
            {
                index--;
                int selectedId = ids[index];

                Item editedItem;
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT Nazwa, Ilosc, Cena FROM Przedmioty WHERE Id = @id";
                    command.Parameters.AddWithValue("@id", selectedId);
                    SqliteDataReader reader = command.ExecuteReader();
                    reader.Read();
                    editedItem = new Item(
                        Convert.ToString(reader["Nazwa"]) ?? "",
                        Convert.ToInt32(reader["Ilosc"]),
                        Convert.ToDecimal(reader["Cena"]));
                }

                Console.WriteLine("Podaj nową nazwę (popraw i wciśnij Enter):");
                Console.Write(editedItem.Name);
                string newName = editedItem.Name;
                while (true)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                    else if (key.Key == ConsoleKey.Backspace && newName.Length > 0)
                    {
                        newName = newName.Substring(0, newName.Length - 1);
                        Console.Write("\b \b");
                    }
                    else if (key.KeyChar != '\0')
                    {
                        newName += key.KeyChar;
                        Console.Write(key.KeyChar);
                    }
                }
                Console.WriteLine();
                if (newName != "")
                {
                    editedItem.Name = newName;
                }

                Console.WriteLine($"Podaj nową ilość (Enter = zostaw {editedItem.Quantity}):");
                string newQuantityText = Console.ReadLine() ?? "";
                if (newQuantityText != "")
                {
                    int newQuantity;
                    while (!int.TryParse(newQuantityText, out newQuantity) || newQuantity <= 0)
                    {
                        Console.WriteLine("❌ Nieprawidłowa liczba! Podaj ilość jeszcze raz:");
                        newQuantityText = Console.ReadLine() ?? "";
                    }
                    editedItem.Quantity = newQuantity;
                }

                Console.WriteLine($"Podaj nową cenę (Enter = zostaw {editedItem.Price}):");
                string newPriceText = Console.ReadLine() ?? "";
                if (newPriceText != "")
                {
                    decimal newPrice;
                    while (!decimal.TryParse(newPriceText, out newPrice) || newPrice <= 0)
                    {
                        Console.WriteLine("❌ Nieprawidłowa liczba! Podaj cenę jeszcze raz:");
                        newPriceText = Console.ReadLine() ?? "";
                    }
                    editedItem.Price = newPrice;
                }

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Przedmioty SET Nazwa = @name, Ilosc = @quantity, Cena = @price WHERE Id = @id";
                    command.Parameters.AddWithValue("@name", editedItem.Name);
                    command.Parameters.AddWithValue("@quantity", editedItem.Quantity);
                    command.Parameters.AddWithValue("@price", editedItem.Price);
                    command.Parameters.AddWithValue("@id", selectedId);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
    else if (choice == "4")      // Usuwanie przedmiotu
    {
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();  
            command.CommandText = "SELECT Id, Nazwa, Ilosc, Cena FROM Przedmioty"; 
            SqliteDataReader reader = command.ExecuteReader();  
            List<int> ids = new List<int>();    
            List<string> names = new List<string>();    
            int number = 1;  
            Console.WriteLine("=== Wybierz przedmiot do usunięcia ===");    
            while (reader.Read()) 
            {
                ids.Add(Convert.ToInt32(reader["Id"])); 
                names.Add(Convert.ToString(reader["Nazwa"]) ?? ""); 
                Console.WriteLine($"{number}. {reader["Nazwa"]} | {reader["Ilosc"]} szt. | {reader["Cena"]} zł" 
                    ); 
                number++;
            }
            if (ids.Count == 0)
            {
                Console.WriteLine("📭 Magazyn jest pusty, nie ma czego usuwać.");

            }
            else 
            {
                Console.WriteLine("Podaj numer przedmiotu do usunięcia:"); 
                string deleteChoice = Console.ReadLine() ?? ""; 
                int index; 
                if (!int.TryParse(deleteChoice, out index) || index < 1 || index > ids.Count)
                {
                    Console.WriteLine("❌ Nieprawidłowy numer!");
                }
                else 
                {
                    index--; 
                    int id = ids[index];
                    SqliteCommand deleteCommand = connection.CreateCommand();
                    deleteCommand.CommandText = "DELETE FROM Przedmioty WHERE Id = @id";
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    deleteCommand.ExecuteNonQuery();
                    Console.WriteLine($"✅ Usunięto przedmiot: {names[index]}");
                    
                }
            }
        }
    }

    else if (choice == "5")      // Obliczanie wartości magazynu
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
                decimal total = Convert.ToDecimal(reader[0]);
                Console.WriteLine($"💰 Wartość magazynu: {total} zł");
            }
        }
    }
    else if  (choice == "6")         // PZ Przyjęcie dostawy
    {
        List<int> ids = new List<int>();
        List<decimal> prices = new List<decimal>();
        using (SqliteConnection connection = new SqliteConnection (connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nazwa, Cena FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            int number = 1;
            Console.WriteLine("=== Wybierz artykuł dla dostawy ===");
            while (reader.Read())
            {
                ids.Add(Convert.ToInt32(reader["Id"]));
                prices.Add(Convert.ToDecimal(reader["Cena"]));
                Console.WriteLine($"{ number}. { reader["Nazwa"]}");
                number++;
            }
        }
        if (ids.Count == 0)
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie można przyjąć dostawy.");
        }
        else
        {
            Console.WriteLine("Podaj numer artykułu:");
            string articleChoice = Console.ReadLine() ?? "";
            int index;
            if (!int.TryParse(articleChoice, out index) || index < 1 || index > ids.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else
            {
                index--;
                int selectedId = ids[index];
                decimal oldPrice = prices[index];

                Console.WriteLine("Podaj ilość przyjętych sztuk:");
                string quantityText = Console.ReadLine() ?? "";
                int quantity;
                while (!int.TryParse(quantityText, out quantity) || quantity <= 0)
                {
                    Console.WriteLine("❌ Nieprawidłowa liczba! podaj ilość jeszcze raz:");
                    quantityText = Console.ReadLine() ?? "";
                }

                Console.WriteLine($"Podaj cenę za sztukę (Enter = {oldPrice} zł):");
                string priceText = Console.ReadLine() ?? "";
                decimal price = oldPrice;
                if (priceText != "")
                {
                    while (!decimal.TryParse(priceText, out price) || price <= 0)
                    {
                        Console.WriteLine("❌ Nieprawidłowa liczba! podaj cenę jeszcze raz:");
                        priceText = Console.ReadLine() ?? "";
                    }
                }
                Console.WriteLine("Podaj numer partii (np. KOSIARKA-0822):");
                string batchNumber = Console.ReadLine() ?? "";
            
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "INSERT INTO Partie (PrzedmiotId, Ilosc, Cena, Data, Status, BatchNumber) VALUES (@itemId, @quantity, @price, @date, @status, @batchNumber)";
                    command.Parameters.AddWithValue("@itemId", selectedId);
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@price", price);
                    command.Parameters.AddWithValue("@date", DateTime.Today.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@status", "Przyjete");
                    command.Parameters.AddWithValue("@batchNumber", batchNumber);
                    command.ExecuteNonQuery();
                }
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Przedmioty SET Ilosc = Ilosc + @quantity WHERE Id = @itemId";
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@itemId", selectedId);
                    command.ExecuteNonQuery();
                }
                Console.WriteLine("✅ Przyjęto dostawę do magazynu.");
            }
        }
    }


    else if (choice == "7")      // WZ — wydanie towaru (FIFO)
    {
        List<int> ids = new List<int>();
        using (SqliteConnection connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            SqliteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nazwa FROM Przedmioty";
            SqliteDataReader reader = command.ExecuteReader();
            int number = 1;
            Console.WriteLine("=== Wybierz artykuł do wydania ===");
            while (reader.Read())
            {
                ids.Add(Convert.ToInt32(reader["Id"]));
                Console.WriteLine($"{number}. {reader["Nazwa"]}");
                number++;
            }
        }
        if (ids.Count == 0) 
        {
            Console.WriteLine("📭 Magazyn jest pusty, nie można wydać towaru.");
        }
        else
        {
            Console.WriteLine("Podaj numer artykułu:");
            string articleChoice = Console.ReadLine() ?? "";
            int index;
            if (!int.TryParse(articleChoice, out index) || index < 1 || index > ids.Count)
            {
                Console.WriteLine("❌ Nieprawidłowy numer!");
            }
            else 
            {
                index--;
                int selectedId = ids[index];

                Console.WriteLine("Podaj ilość do wydania:");
                string quantityText = Console.ReadLine() ?? "";
                int quantity;
                while (!int.TryParse(quantityText, out quantity) || quantity <= 0)
                {
                    Console.WriteLine("❌ Nieprawidłowa liczba! podaj ilość jeszcze raz:");
                    quantityText = Console.ReadLine() ?? "";
                }

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Ilosc, BatchNumber FROM Partie WHERE PrzedmiotId = @itemId AND Ilosc > 0 ORDER BY Id ASC";
                    command.Parameters.AddWithValue("@itemId", selectedId);
                    SqliteDataReader reader = command.ExecuteReader();
                    Console.WriteLine("Partie (najstarsza pierwsza):");
                    while (reader.Read())
                    {
                        Console.WriteLine($"  [{reader["BatchNumber"]}] {reader["Ilosc"]} szt");    
                    }
                }
                int remaining = quantity;
                List<int> partIds = new List<int>();
                List<int> batchSizes = new List<int>();
                List<string> batchNumbers = new List<string>();
                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "SELECT Id, Ilosc, BatchNumber FROM Partie WHERE PrzedmiotId = @itemId AND Ilosc > 0 ORDER BY Id ASC";
                    command.Parameters.AddWithValue("@itemId", selectedId);
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        partIds.Add(Convert.ToInt32(reader["Id"]));
                        batchSizes.Add(Convert.ToInt32(reader["Ilosc"]));
                        batchNumbers.Add(Convert.ToString(reader["BatchNumber"]) ?? "");

                    }
                }

                int i = 0;
                while (i < partIds.Count && remaining > 0)
                {
                    int partId = partIds[i];
                    int inBatch = batchSizes[i];
                    Console.WriteLine($"Wydaję z [{batchNumbers[i]}]...");
                    if (inBatch <= remaining)
                    {
                        using (SqliteConnection conn2 = new SqliteConnection(connectionString))
                        {
                            conn2.Open();
                            SqliteCommand cmd2 = conn2.CreateCommand();
                            cmd2.CommandText = "UPDATE Partie SET Ilosc = 0, Status = 'Wydane' WHERE Id = @partId";
                            cmd2.Parameters.AddWithValue("@partId", partId);
                            cmd2.ExecuteNonQuery();
                        }
                        remaining -= inBatch;

                    }
                    else
                    {
                        using (SqliteConnection conn2 = new SqliteConnection(connectionString))
                        {
                            conn2.Open();
                            SqliteCommand cmd2 = conn2.CreateCommand();
                            cmd2.CommandText = "UPDATE Partie SET Ilosc = Ilosc - @take WHERE Id = @partId";
                            cmd2.Parameters.AddWithValue("@take", remaining);
                            cmd2.Parameters.AddWithValue("@partId", partId);
                            cmd2.ExecuteNonQuery();
                        }
                        remaining = 0;
                    }
                    i++;
                }

                using (SqliteConnection connection = new SqliteConnection(connectionString))
                {
                    connection.Open();
                    SqliteCommand command = connection.CreateCommand();
                    command.CommandText = "UPDATE Przedmioty SET Ilosc = Ilosc - @quantity WHERE Id = @itemId";
                    command.Parameters.AddWithValue("@quantity", quantity);
                    command.Parameters.AddWithValue("@itemId", selectedId);
                    command.ExecuteNonQuery();
                }

                if (remaining > 0)
                {
                    Console.WriteLine("❌ Za mało towaru w magazynie!");
                }
                else
                {
                    Console.WriteLine("✅ Wydano towar z magazynu.");
                }

            }
        }
    }

    else if (choice == "8")      // Wyjście z programu
    {
        break;
    }
    else
    {
        Console.WriteLine("❌ Nie ma takiej opcji!");
    }
}
