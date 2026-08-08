# Nauka C# — Magazyn, dzień 5 (08.08.2026)

## 1. SQLite — baza danych

- Do tej pory przedmioty żyły w liście w pamięci — **znikały po zamknięciu programu**
- SQLite to **baza danych w pliku** (`magazyn.db`) — dane przetrwają zamknięcie i ponowne uruchomienie
- Bez serwera i bez instalacji — baza to po prostu plik na dysku
- Do projektu dodaliśmy pakiet `Microsoft.Data.Sqlite` (to on daje nam klasy `SqliteConnection`, `SqliteCommand`…)

## 2. Połączenie z bazą

```csharp
string connectionString = "Data Source=magazyn.db";
using (SqliteConnection connection = new SqliteConnection(connectionString))
{
    connection.Open();
    // ... tu pracujemy z bazą
}
```

| Pojęcie | Co robi |
|---------|---------|
| `connectionString` | „adres" bazy — `Data Source=...` mówi, w którym pliku jest baza |
| `SqliteConnection` | obiekt reprezentujący **połączenie** z bazą |
| `connection.Open()` | otwórz połączenie (zanim cokolwiek zrobimy) |
| `using ( ... )` | po wyjściu z klamer połączenie **samo się zamyka** — nie musimy o tym pamiętać |

## 3. CREATE TABLE IF NOT EXISTS — tabela przy starcie

```csharp
command.CommandText = @"
    CREATE TABLE IF NOT EXISTS Przedmioty (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Nazwa TEXT NOT NULL,
    Ilosc INTEGER NOT NULL,
    Cena REAL NOT NULL
    )";
```

- Tabela to jak arkusz: **wiersz = jeden przedmiot**, kolumny = jego właściwości
- Typy kolumn: `INTEGER` = liczba całkowita, `TEXT` = tekst, `REAL` = liczba z przecinkiem
- `PRIMARY KEY AUTOINCREMENT` — baza **sama nadaje numer** Id każdemu wierszowi
- `NOT NULL` — pole nie może zostać puste
- `IF NOT EXISTS` — „utwórz tylko jeśli jeszcze nie ma" — przy każdym starcie nie zepsuje istniejącej tabeli

## 4. INSERT — zapis nowego przedmiotu z parametrami

```csharp
command.CommandText = "INSERT INTO Przedmioty (Nazwa, Ilosc, Cena) VALUES (@nazwa, @ilosc, @cena)";
command.Parameters.AddWithValue("@nazwa", nazwa);
command.Parameters.AddWithValue("@ilosc", ilosc);
command.Parameters.AddWithValue("@cena", cena);
command.ExecuteNonQuery();
```

- `INSERT INTO Przedmioty (Nazwa, Ilosc, Cena) VALUES (...)` — „dodaj wiersz do tabeli"
- `@nazwa`, `@ilosc`, `@cena` — **parametry**: dziury w tekście, do których potem wstawiamy wartości
- `AddWithValue("@nazwa", nazwa)` — podpięcie prawdziwej wartości pod parametr
- **Czemu parametry, a nie sklejanie tekstu?** Bo wartości trafiają do bazy jako „dane", a nie jako „część rozkazu" — bezpieczniej i baza nie pomyli się przy cudzysłowach i przecinkach
- `ExecuteNonQuery()` — „wykonaj rozkaz i nie oczekuj wyniku" (używamy go do INSERT / UPDATE / DELETE)

## 5. SELECT + SqliteDataReader — odczyt z bazy (w trakcie!)

```csharp
command.CommandText = "SELECT Nazwa, Ilosc, Cena FROM Przedmioty";
SqliteDataReader reader = command.ExecuteReader();
while (reader.Read())
{
    Console.WriteLine($"- {reader["Nazwa"]} | Ilość: {reader["Ilosc"]} | Cena: {reader["Cena"]} zł");
}
```

| Pojęcie | Co robi |
|---------|---------|
| `SELECT ... FROM ...` | „pokaż te kolumny z tabeli" — zapytanie, które **zwraca** dane |
| `ExecuteReader()` | wykonaj zapytanie i zwróć **czytnik** wyników |
| `SqliteDataReader` | czytnik — przechodzi po wierszach wyniku, jak kursor |
| `reader.Read()` | przejdź do następnego wiersza; zwraca `false`, gdy wynik się skończył |
| `reader["Nazwa"]` | wartość kolumny `Nazwa` z bieżącego wiersza |

## 6. Błąd dnia — w C# wielkość liter ma znaczenie

- `SqlitedataReader` ❌ vs `SqliteDataReader` ✅ — różni je **wielka litera D**
- C# rozróżnia wielkie i małe litery — dla kompilatora to dwie **różne nazwy**, a istnieje tylko jedna z nich
- Stąd błąd: `CS0246: Nie można znaleźć nazwy typu` — kompilator szuka klasy, której nie ma
- W Visual Studio taki błąd podkreśla się na czerwono **od razu podczas pisania** — `dotnet run` łapie go dopiero na końcu

## Stan projektu / plan na następną sesję

- ✅ Dodawanie przedmiotu → zapisuje do bazy (INSERT)
- 🚧 Wyświetlanie → czyta z bazy (SELECT) — **do dokończenia** (do poprawy literówka z punktu 6)
- ⏳ Edycja i usuwanie → na razie działają tylko na liście w pamięci, **nie zmieniają bazy**
- Następnym razem: `UPDATE` (edycja) i `DELETE` (usuwanie) w bazie, żeby wszystkie opcje menu działały na zapisanych danych
- Do rozważenia: aktualizacja pakietu SQLite (kompilator ostrzegał o luce bezpieczeństwa)
