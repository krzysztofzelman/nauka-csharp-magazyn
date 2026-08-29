# Nauka C# — Magazyn, dzień 6 (09.08.2026)

## 1. Test restartu — dane przeżywają zamknięcie programu ✅

- Przedmioty dodane wczoraj (INSERT do bazy), program zamknięty
- Dziś świeży start → opcja 2 pokazała wszystkie 6 przedmiotów **prosto z pliku `magazyn.db`**
- To był cel: **nie wpisywać pozycji za każdym razem od nowa** — dane siedzą w pliku, nie w pamięci

## 2. Powtórka — "piosenka" wszystkich bloków SQLite

Każdy blok SQLite to te same 3 kroki:

```
1. Otwórz połączenie   →  using (SqliteConnection ...) { connection.Open();
2. Przygotuj polecenie →  SqliteCommand command = connection.CreateCommand(); command.CommandText = "...";
3. Wykonaj             →  command.Execute...();
```

Klamra `}` na końcu = **exit** — połączenie zamyka się samo (jak drzwi automatyczne).

## 3. Rozkaz vs pytanie (ExecuteNonQuery vs ExecuteReader)

| SQL | Co to jest | Które Execute |
|-----|------------|---------------|
| INSERT / CREATE TABLE / UPDATE / DELETE | **rozkaz** ("Dodaj przedmiot!") — nic nie oddaje | `ExecuteNonQuery()` |
| SELECT | **pytanie** ("Co jest w magazynie?") — oddaje odpowiedź do czytania | `ExecuteReader()` |

> **SELECT to JEDYNE pytanie w SQL.** Pytanie → dostajesz odpowiedź → czytasz (while reader.Read()).

## 4. Opcja 4 (Usuwanie) — krok 1: lista z bazy z numerkami Id

```csharp
command.CommandText = "SELECT Id, Nazwa, Ilosc, Cena FROM Przedmioty";
SqliteDataReader reader = command.ExecuteReader();
List<int> idy = new List<int>();        // parking na numery Id
List<string> nazwy = new List<string>(); // parking na nazwy
int numer = 1;
while (reader.Read())
{
    idy.Add(Convert.ToInt32(reader["Id"]));      // Id → liczba → parking
    nazwy.Add(Convert.ToString(reader["Nazwa"]) ?? "");  // nazwa → parking
    Console.WriteLine($"{numer}. {reader["Nazwa"]} | ...");
    numer++;
}
```

| Pojęcie | Co robi |
|---------|---------|
| Kolumna `Id` w SELECT | numerek ewidencyjny z bazy (AUTOINCREMENT) — po nim będziemy usuwać |
| `List<int> idy` | **parking** numerów Id — numer 1 na ekranie = `idy[0]`, numer 2 = `idy[1]`... |
| `List<string> nazwy` | parking nazw — w tej samej kolejności co Id |
| `Convert.ToInt32(reader["Id"])` | reader oddaje wartości "bez typu" → zamiana na liczbę |
| `Convert.ToString(...) ?? ""` | zamiana na tekst + "gdyby było puste, wstaw pusty tekst" (uspokaja kompilator) |
| `numer` | licznik numerków widocznych na ekranie |

## 5. Błąd dnia — brakująca klamra (pudełka w pudełkach)

- Kod miał: `if` → `using` → gałąź opcji 4 — **trzy pudełka, każdemu trzeba zamknąć wieko**
- Zabrakło jednej `}` (zamknięcia bloku `using`) → kompilator nie przechodził
- Poprawka: dopisanie jednej klamry w odpowiednim miejscu ✅

## 6. Git — commit i push

- Commit `27970b1`: "Opcja 4 usuwanie: lista z bazy (SELECT z Id) - krok 1/2" — wypchnięty na GitHub ✅
- Notatki z dnia 5 zaktualizowane (SELECT oznaczony ✅)

## ⚠️ Znaleziony problem: DWA źródła prawdy (naprawić następnym razem)

Dane są trzymane w dwóch miejscach naraz, które się rozjeżdżają:

| Opcja | Czyta z | Pisze do |
|-------|---------|----------|
| 1 (Dodaj) | — | lista ✅ + baza ✅ |
| 2 (Wyświetl) | baza | — |
| 3 (Edytuj) | **lista** | **lista** — baza nic nie widzi |
| 4 (Usuń) | baza | — (krok 2 czeka) |
| 5 (Wartość) | **lista** | — |

**Efekt (test restartu):** lista w pamięci startuje pusta, a baza ma dane → opcja 2 pokaże przedmioty z bazy, a opcja 5 pokaże **0 zł**. Edycja w opcji 3 "znika" po sprawdzeniu w opcji 2.

**Decyzja:** `List<Item> magazyn` znika z kodu — **baza = JEDYNE źródło prawdy**. Migracja opcji 3 i 5 NAJPIERW, dopiero potem krok 2 usuwania (nie budować na fundamencie do wyburzenia).

## Plan na następną sesję (nowa kolejność!)

1. **Opcja 3 (Edycja) → baza:**
   - **Krok 1 — SELECT wiersza:** pobierz cały wiersz z bazy po Id (drugie zapytanie: `SELECT ... WHERE Id = @id` — drugie użycie `WHERE`, pierwsze było w planowanym DELETE). Użyj wartości z bazy jako domyślnych w promptach ("Enter = zostaw {stara_wartość}" — ten sam wzorzec promptów, tylko źródłem domyślnych jest reader zamiast `magazyn[indeks]`).
   - **Krok 2 — UPDATE wszystkich kolumn:** `UPDATE Przedmioty SET Nazwa=@n, Ilosc=@i, Cena=@c WHERE Id = @id`. Wszystkie trzy kolumny za każdym razem, nawet jeśli user nic nie zmienił — kod jest prostszy, a różnica wydajnościowa przy tym projekcie zero.
   - **Backspace na liczbach → wyrzucić.** ReadLine + TryParse w pętli masz już w 4 miejscach (ilość/cena przy dodawaniu, ilość/cena przy edycji) — ujednolicenie do jednego wzorca = mniej niespodzianek przy czytaniu kodu. Backspace z `ConsoleKeyInfo` zostaje tylko dla nazwy (tekst), gdzie ma sens.
2. **Opcja 5 (Wartość) → baza:** `SELECT SUM(Ilosc * Cena) FROM Przedmioty` — jedno pytanie, zero pętli. ⚠️ Dwie pułapki:
   - **Pułapka 1 — typ:** SQLite nie ma `decimal`, `REAL` to `double` w środku. `reader[0]` odda `double`, nie `decimal`. Bezpośrednie rzutowanie `(decimal)reader[0]` → ❌ `InvalidCastException`. **Rozwiązanie:** `Convert.ToDecimal(reader[0])` — ten sam wzorzec co `Convert.ToInt32(reader["Id"])` w opcji 4, tylko inny typ docelowy.
   - **Pułapka 2 — pusta tabela:** `SUM` na pustej tabeli zwraca `NULL`, nie `0`. W C# reader odda to jako `DBNull`. Próba `Convert.ToDecimal` na `DBNull` → ❌ wyjątek zamiast "0 zł". **Rozwiązanie:** `if (reader.IsDBNull(0))` przed konwersją — jeśli `true`, wartość = `0`.
3. **Sprzątanie:** usunąć `List<Item> magazyn`; los `Funkcje.WyswietlListe` / `WartoscMagazynu` / `Item.cs` do decyzji
4. **Opcja 4, krok 2:** `DELETE FROM Przedmioty WHERE Id = @id` — nowy koncept: **WHERE** ("gdzie" — usuń TYLKO ten wiersz)

## 7. Ciąg dalszy dnia 6 — opcja 3 (Edycja) → baza ✅

### Co zrobiono
Opcja 3 przerobiona z listy na bazę — teraz działa w 3 krokach:

1. **SELECT** — lista przedmiotów z bazy z numerkami (parking `idy`) — wzorzec z opcji 4
2. **SELECT ... WHERE Id = @id** — po wybraniu numeru pobiera JEDEN wiersz z bazy i z niego buduje `Item edytowany` → stąd biorą się domyślne wartości w promptach ("Enter = zostaw 780")
3. **UPDATE ... WHERE Id = @id** — na końcu wysyła do bazy nowe wartości (nazwa, ilość, cena)

Część z backspace'em, ilością i ceną została **nietknięta** — dalej operuje na `edytowany`, tylko teraz ten obiekt pochodzi z bazy.

### Test — sukces 🎉
- Edycja: Glebogryzarka 1 szt. 780 zł → **3 szt. 830 zł**
- Opcja 2 pokazała zmianę ✅
- **Restart programu → zmiana przetrwała** ✅ (wcześniej znikała, bo była tylko na liście)

### Nowe koncepty
| Pojęcie | Co to jest |
|---------|------------|
| `WHERE Id = @id` | **pierwsze użycie WHERE w SELECT** — "daj mi TYLKO ten jeden wiersz o danym Id" |
| `UPDATE ... SET ... WHERE Id = @id` | **rozkaz do bazy** (ExecuteNonQuery) — "zmień te kolumny w tym wierszu" |
| `Item edytowany;` + `new Item(...)` z danych z reader | budowanie obiektu z wiersza bazy (Convert.ToString / ToInt32 / ToDecimal) |
| Błąd CS0136 | nie można dwa razy deklarować `connection`/`command`/`reader` w zagnieżdżonych `using` → **naprawa:** pierwszy `using` zamykamy zaraz po pętli `while`, reszta idzie poza nim |

### Pułapki
- **CS0136** — trzy bloki `using` z tymi samymi nazwami zmiennych nie mogą być zagnieżdżone. Rozwiązanie: każde zapytanie ma SWÓJ osobny blok `using`, ale nie w środku innego.
- **Autouzupełnianie VS** — przeszkadzało przy pisaniu (dopisywało/wykasowywało fragmenty). Wyłączone w: Narzędzia → Opcje → Edytor tekstu → C# → IntelliSense (odhaczyć "Pokaż listę uzupełniania"). Podpowiedź ręcznie: **Ctrl+Spacja**.
- Literówki po wyłączeniu podpowiadacza: `comannd.ComanndText` → `command.CommandText` — trzeba czytać kod wzrokiem.

## 8. Podsumowanie nauki (stan na koniec dnia 6)

### Co potrafię teraz
- **Pełny obieg CRUD na bazie SQLite:** Dodać (INSERT) → Wyświetlić (SELECT) → Edytować (UPDATE) → lista do usuwania (SELECT + parking Id)
- "Piosenka" SQLite: Open → CreateCommand → CommandText → Execute → (Read)
- **Rozkaz vs pytanie:** ExecuteNonQuery (INSERT/UPDATE/DELETE) vs ExecuteReader (SELECT)
- **WHERE** — filtrowanie: "tylko ten wiersz"
- **Parametry @nazwa @id** — bezpieczne wstawianie wartości do zapytań
- **Parkingi (List<T>)** — numerki na ekranie ≠ Id w bazie: `numer 1 = idy[0]`
- **Convert.ToInt32 / Convert.ToString / Convert.ToDecimal** — przerabianie wartości z bazy na typy C#
- **Baza = jedyne źródło prawdy** — edycja przetrwa restart ✅

### Wzorzec opcji na bazie (3 kroki)
```
1. SELECT + parking Id   → pokaż listę z numerkami
2. SELECT ... WHERE Id   → pobierz wybrany wiersz
3. UPDATE ... WHERE Id   → zapisz zmiany
```

### Zostało na następną sesję (nowa kolejność!)
1. **Opcja 5 (Wartość) → baza:** `SELECT SUM(Ilosc * Cena) FROM Przedmioty` — ⚠️ pułapki: `Convert.ToDecimal` (REAL to double, nie decimal!) + `IsDBNull` (pusta tabela → SUM zwraca NULL)
2. **Sprzątanie:** usunąć `List<Item> magazyn` (opcja 1 przestaje dodawać do listy), los `Funkcje.cs` / `Item.cs`
3. **Opcja 4, krok 2:** `DELETE FROM Przedmioty WHERE Id = @id` — drugie użycie WHERE
4. **Notatki:** zaktualizować status w tym pliku i ewentualnie zaktualizować pakiet SQLite (NU1903)

### Status

- ✅ Dodawanie → baza (INSERT)
- ✅ Wyświetlanie → baza (SELECT)
- ✅ Usuwanie: krok 1 (lista z bazy + parkingi Id)
- ✅ **Edycja → baza (SELECT po Id + UPDATE)** — przetrwała restart
- ✅ **Usuwanie: krok 2 (DELETE po Id)** — dzień 7, patrz niżej
- ⏳ Wartość magazynu → do migracji (na razie liczy z listy)

---

# Dzień 7 (10.08.2026) — opcja 4, krok 2: DELETE ✅

## Co zrobiono

Dopisana właściwa część usuwania w opcji 4 — po wybraniu numeru:

```csharp
indeks--;
int id = idy[indeks];                        // parking: numer z ekranu → prawdziwe Id z bazy
SqliteCommand komendaUsun = connection.CreateCommand();
komendaUsun.CommandText = "DELETE FROM Przedmioty WHERE Id = @id";
komendaUsun.Parameters.AddWithValue("@id", id);
komendaUsun.ExecuteNonQuery();
Console.WriteLine($"✅ Usunięto przedmiot: {nazwy[indeks]}");
```

## Nowy koncept

| Pojęcie | Co to jest |
|---------|------------|
| `DELETE FROM ... WHERE Id = @id` | **rozkaz** (ExecuteNonQuery): „skasuj TYLKO ten wiersz, gdzie Id pasuje". Bez `WHERE` skasowałbyś **całą tabelę** — WHERE to bezpiecznik. |

## Dlaczego wokół jednej linii jest tyle kodu?

Bo samo usuwanie to jedna linia (`DELETE...`), reszta to obsługa:

- **SELECT + parkingi** — user musi widzieć listę, żeby wybrać (baza rozumie tylko Id, nie numerki)
- **walidacja** — user nie wpisze „abc" ani `99`
- **`int id = idy[indeks]`** — tłumaczenie numeru (3) na prawdziwe Id (np. 37)
- **komunikat** — user widzi, że się udało

Analogia: **kuchnia (baza)** zna tylko „danie nr 37" (Id), **klient (user)** zna „trzecie na liście" — kod to **kelner**, który tłumaczy jedno na drugie.

## Test — sukces 🎉

- Dodany „Test" 1×1 → opcja 4, numer 7 → `✅ Usunięto przedmiot: Test` → opcja 2: **zniknął z bazy** (opcja 2 czyta z pliku, więc to dowód na kasowanie w bazie) ✅
- Walidacja: wpisanie `abc` → `❌ Nieprawidłowy numer!` — program się nie wysypał ✅

## Błędy dnia

1. **Brakująca klamra `{`** po `if (!int.TryParse(...))` — była `}` zamykająca, nie było otwierającej → „else bez if". Wzorzec „pudełka w pudełkach" (drugi raz).
2. **Literówka `koemndaUsun`** → `komendaUsun` — zmienna ma JEDNO imię przez cały kod; deklaracja i użycie muszą się zgadzać co do znaku.
3. Lekcja: **imię parametru w `AddWithValue` musi się zgadzać z dziurą w SQL co do znaku** — `"@id"` ≠ `" @id"`.

## Status (koniec dnia 7)

- ✅ Dodawanie → baza (INSERT)
- ✅ Wyświetlanie → baza (SELECT)
- ✅ Edycja → baza (UPDATE po Id)
- ✅ **Usuwanie → baza (DELETE po Id) — cały CRUD na bazie!**
- ⏳ Wartość magazynu → do migracji (`SELECT SUM(Ilosc * Cena)` + pułapki DBNull/decimal)
- ⏳ Sprzątanie: `List<Item> magazyn`, los `Funkcje.cs` / `Item.cs`

---

# Dzień 8 (11.08.2026) — podstawy na moim kodzie + parking Id/id/idy ✅

Dziś zero nowego kodu — za to teoria NA WŁASNYM kodzie (nie z kursu) + domknięty temat, który wczoraj był "mętnie".

## 1. Zasada: najpierw TYP, potem IMIĘ

```
typ imię = wartość;
```

- `string nazwa = "Kosiarka";` → typ: string, imię: nazwa
- `int ilosc;` → typ: int, imię: ilosc
- `decimal cena;` → typ: decimal, imię: cena

**Trik "patrz na wartość"** — jak poznać typ bez pamiętania:

| Widzisz | Typ |
|---------|-----|
| tekst w cudzysłowie `"..."` | string |
| liczba bez przecinka `5` | int |
| liczba z przecinkiem `12,50` | decimal |

## 2. List<T> — lista ("pudełko")

```csharp
List<Item> magazyn = new List<Item>();
List<int> idy = new List<int>();
List<string> nazwy = new List<string>();
```

- Ten sam wzorzec co zwykła zmienna: typ + imię. `< >` mówi, czego może być dużo w środku.
- **Klucz: "pudełko TO JEST zmienna"** — wszystko z imieniem to zmienna; różnica tylko w tym, ILE wartości mieści: zwykła zmienna JEDNĄ, lista WIELE.
- `new List<...>()` = "zbuduj nową, pustą listę" — musi być zbudowana, zanim cokolwiek dodasz (`Add`).
- `int ilosc;` nie potrzebuje `new` — jedna liczba mieści się sama.

## 3. Parking Id / id / idy — ZALICZONY ✅ (wczoraj: "mętnie")

| Nazwa | Co to jest |
|-------|------------|
| `Id` | **NIE jest zmienną.** To nazwa kolumny W BAZIE (z `CREATE TABLE ... Id INTEGER PRIMARY KEY AUTOINCREMENT`). Żyje tylko w SQL i w cudzysłowach: `SELECT Id`, `WHERE Id = @id`, `reader["Id"]` |
| `id` | zwykła zmienna na JEDNĄ liczbę: `int id = idy[indeks];` |
| `idy` | lista na WIELE numerów |

### Poprawka, którą warto zapamiętać
**Edycja NIE tworzy nowego Id.** Id baza nadaje RAZ, przy dodawaniu. `UPDATE ... WHERE Id = @id` zmienia wartości w TYM SAMYM wierszu. Usunięcie kasuje wiersz, a jego numer NIGDY nie wraca → dlatego bywają dziury (1, 2, 5, 9) — to nie "stare dane", tylko usunięte wiersze. **Id = stała tożsamość wiersza.**

### while (reader.Read()) — słowo po słowie
- `reader` — wynik zapytania SELECT (tabelka), który dostałeś z `ExecuteReader()`
- `Read()` — "przestaw się na następny wiersz i powiedz, czy jest": `true` = jest, `false` = koniec
- `while` — powtarzaj, dopóki `true`. Czytnik startuje PRZED pierwszym wierszem.
- Dlatego `while`, nie `if`: `if` sprawdza raz, a wierszy nie znasz z góry.

### Po co lista `idy`?
**Numer wpisany przez usera ≠ Id w bazie.** User wybiera "3" (trzecia linia na ekranie), a baza zna tylko swoje Id (np. 9). Lista to most kolejności: **linia N na ekranie ↔ `idy[N-1]`**. Bez listy program po wypisaniu zapomina, która linia miała które Id.

## 4. Nadpisywanie — moment "aaa teraz kumam" 💡

- `=` **NADPISUJE** — zmienna ma JEDNO miejsce:
  - obrót 1: `id = 3` → w zmiennej: 3
  - obrót 2: `id = 8` → w zmiennej: 8 (3 zniknęła!)
  - obrót 3: `id = 12` → w zmiennej: 12 (8 zniknęła!)
  - po pętli zostaje tylko OSTATNIA wartość
- `Add` **DOPISUJE** — lista rośnie: [3] → [3, 8] → [3, 8, 12]. Nic nie znika.

Nadpisywanie nie jest błędem: `string wybor = Console.ReadLine()` nadpisuje się co obrót menu i to OK, bo potrzebujesz tylko ostatniego wyboru. **Listy używasz wtedy, gdy potrzebujesz WSZYSTKICH wartości naraz.**

## 5. Opcja 5 — co to jest (wyjaśnione, migracja na jutro)

- "💰 Wartość magazynu" = ile warte jest wszystko w magazynie: suma ilość × cena
- Teraz liczy funkcja `WartoscMagazynu` (Funkcje.cs) z LISTY w pamięci → po restarcie pokaże **0 zł**, bo lista startuje pusta
- **Jutro:** migracja do bazy — `SELECT SUM(Ilosc * Cena) FROM Przedmioty`

## Status (koniec dnia 8)

- ✅ Podstawy: typ+imię, trik "patrz na wartość", List<T>
- ✅ Parking Id/id/idy — wczoraj "mętnie", dziś zaliczony (sam opisałem nadpisywanie własnymi słowami)
- ⏳ **Opcja 5 → baza (SUM + Convert.ToDecimal + IsDBNull) — NASTĘPNA SESJA**
- ⏳ Sprzątanie: `List<Item> magazyn`, los `Funkcje.cs` / `Item.cs`

# Dzień 9 (12.08.2026) — opcja 5 na bazie + sprzątanie ✅

## Co zrobiono

1. **Opcja 5 (Wartość magazynu) → baza.** Zamiana `Funkcje.WartoscMagazynu(magazyn)` (liczyła z listy w pamięci) na zapytanie do bazy:

```csharp
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
```

2. **Sprzątanie:** usunięte `List<Item> magazyn` (deklaracja + `magazyn.Add(...)` w opcji 1) i cały plik `Funkcje.cs` (martwy kod). `Item.cs` został — opcja 3 go używa (`Item edytowany`).

## Nowe koncepty

- **`SUM(...)`** — SQL sam liczy sumę, zamiast pętli `foreach` w C#. `SUM(Ilosc * Cena)` = "dla każdego wiersza policz ilość×cena i dodaj do kupy". Wynik to **zawsze 1 wiersz z 1 kolumną** → dlatego `reader.Read()` bez `while`.
- **`reader[0]`** — odczyt kolumny po **numerze** zamiast nazwy (kolumna SUM nie ma imienia). `reader[0]` = "w bieżącym wierszu weź kolumnę 0". Jak Excel: najpierw wiersz (na którym stoi Read), potem kolumna (numer).
- **`reader.IsDBNull(0)`** — "czy komórka w kolumnie 0 jest pusta (NULL)?" Pułapka: **pusta tabela → SUM zwraca NULL, a nie 0** — ale wiersz NADAL istnieje! Różnica: `Read()` pyta o wiersz ("czy jest następny?"), `IsDBNull(0)` pyta o komórkę ("czy jest pusta?"). W opcji 2 pusty magazyn = brak wierszy, tu = 1 wiersz z pustą komórką. Bez tego sprawdzenia `Convert.ToDecimal` na pustce → błąd.
- **`Convert.ToDecimal(reader[0])`** — cena w bazie to `REAL` = **double** w C# (SQLite nie zna decimala) → trzeba skonwertować. Ten sam wzorzec Convert co przy `Convert.ToInt32(reader["Id"])`.
- **IntelliSense podpowiadał `ExecuteScalar()`** — VS pokazuje LISTĘ wszystkich metod na obiekcie (ExecuteNonQuery / ExecuteReader / ExecuteScalar), to nie jest nakaz. ExecuteScalar też by zadziałał dla SUM (zwraca od razu jedną wartość), ale wybraliśmy ExecuteReader — ta sama piosenka co opcja 2, zero nowych metod.
- **Martwy kod** — kompiluje się, ale nic go nie woła. Kompilator sam tego nie sprząta (nie zgłosił błędu po zostawieniu Funkcje.cs) — trzeba wynosić ręcznie.

## Test — sukces 🎉

- Opcja 5 przed edycją: **14040 zł**
- Edycja Kosy spalinowej (ilość 3 → 5) przez opcję 3 → opcja 5: **17040 zł** (+3000 = 2×1500)
- **Dowód, że opcja 5 czyta z bazy:** gdyby liczyła ze starej listy, pokazałaby 14040 drugi raz (lista nie wie o edycji). Pokazała świeżą sumę z bazy = dwa źródła prawdy sklejone.
- Po sprzątaniu: dodane "Grabie" 2×60 → lista pokazuje 7 przedmiotów z bazy, opcja 5: **17160 zł** (17040 + 120) ✅

## Status (koniec dnia 9)

- ✅ **Wszystkie opcje (1–5) działają wyłącznie na bazie** — jedyne źródło prawdy
- ✅ `List<Item> magazyn` usunięta, `Funkcje.cs` usunięty (martwy kod), `Item.cs` zostaje
- ✅ Projekt: 2 pliki `.cs` (Program.cs, Item.cs)
- 💡 **DECYZJA O KIERUNKU:** konsola skończona → **ASP.NET Core API** (backend = silnik). Utworzony projekt `MagazynApi` (dotnet new webapi --use-controllers). Frontend dopiero po API — na start **Blazor (w C#)**, TS/React później. Łączenie frontend↔backend przez HTTP. Notatka: `MagazynApi/nauka-dzien-1.md`.

---

# Dzień 16 (24.08.2026) — PZ/WZ w konsoli (start)

## Co się dzieje: PZ = przyjęcie dostawy, WZ = wydanie (FIFO)

Magazyn dostał nowy wymiar: **partie**. Wcześniej był tylko „ile sztuk czego" (Przedmioty). Teraz dochodzi **skąd się to wzięło** — każda dostawa to osobna partia w tabeli `Partie`. WZ (wydanie) będzie brał towar **z najstarszych partii** (FIFO = first in, first out — pierwsze weszło, pierwsze wyszło).

## 1. Tabela Partie — bliźniak Przedmioty ✅

W `Program.cs` dodany drugi blok CREATE TABLE — **identyczny wzorzec**:

```
1. Otwórz połączenie   →  using (SqliteConnection ...) { connection.Open();
2. Przygotuj polecenie →  SqliteCommand command = connection.CreateCommand(); command.CommandText = "...";
3. Wykonaj             →  command.ExecuteNonQuery();
```

```sql
CREATE TABLE IF NOT EXISTS Partie (
Id INTEGER PRIMARY KEY AUTOINCREMENT,   -- numer partii, baza nadaje sama
PrzedmiotId INTEGER,                    -- do KTÓREGO artykułu należy (np. 1 = Kosiarka)
Ilosc INTEGER,                          -- ile sztuk w tej partii
Cena REAL,                              -- cena za sztukę z TEJ dostawy
Data TEXT,                              -- kiedy doszła
Status TEXT,                            -- "Przyjete" / "Wydane" (kluczowe dla FIFO!)
BatchNumber TEXT                        -- ludzki numer partii (np. KOSIARKA-0822)
)
```

- `IF NOT EXISTS` = „jeśli już istnieje, nie rób nic" — bezpieczne przy każdym starcie
- **Schemat MUSI być identyczny z API** (MagazynApi) — bo to **ten sam plik bazy** (`Magazyn\magazyn.db`, patrz appsettings.json API). Partie dodane na stronie /partie konsola od razu zobaczy.
- Tabela już istniała (stworzyło ją API) → ten blok to „samowystarczalność" konsoli + ściąga schematu

## 2. Menu: 6 = PZ, 7 = WZ, 8 = Wyjście ✅

Wyjście musiało przesunąć się z 6 na 8 — program sprawdza warunki po kolei, więc 6 i 7 zajęły PZ i WZ.

## 3. PZ część 1 — lista artykułów (wzorzec opcji 4) ✅

```csharp
List<int> idy = new List<int>();          // parking na Id
using (SqliteConnection connection = ...) // piosenka
{
    ...
    command.CommandText = "SELECT Id, Nazwa FROM Przedmioty";  // tylko to, czego trzeba
    SqliteDataReader reader = command.ExecuteReader();          // pytanie → odpowiedź
    int numer = 1;
    while (reader.Read())                 // dopóki jest wiersz
    {
        idy.Add(Convert.ToInt32(reader["Id"]));   // zapamiętaj Id
        Console.WriteLine($"{numer}. {reader["Nazwa"]}");
        numer++;
    }
}
if (idy.Count == 0)   // pusta lista = pusty magazyn
```

## 4. PZ część 2 — wybór + dane dostawy ✅

- **Wybór numeru** (wzorzec opcji 4): `indeks--` (menu od 1, lista od 0) → `wybraneId = idy[indeks]`
- **Ilość** — pętla walidacji (wzorzec opcji 1): `while (!int.TryParse(...) || ilosc <= 0)`
- **Cena** — to samo z `decimal.TryParse`
- **BatchNumber** — zwykły tekst, bez walidacji

⚠️ **Lekcja dnia:** gdy bloki się zagnieżdżają, łatwo „wjechać" pętlą w pętlę (cena w środku ilości). Ratunek: pisać mniejszymi kawałkami + sprawdzać `sprawdz` co kawałek.

## 5. Obserwacja: „sztywny kod co do połączeń baz"

Piosenka (using → Open → CreateCommand → Execute) jest **powtórzona w każdej opcji** — to duplikacja. Celowa: konsola to ściąga, każdy blok ma całość pod nosem. Profesjonalnie robi się to **jedną metodą-pomocnikiem** (jak `builder.Configuration` w API) — refaktor na później, nie w środku budowy.

## Status (koniec dnia 16)

- ✅ Tabela `Partie` + menu 6/7/8 + PZ części 1–2 (bez zapisu do bazy jeszcze)
- ✅ Build OK, commit `7f865d7` push
- ⏭️ **Następne:** PZ część 3 (INSERT do Partie + UPDATE Przedmioty.Ilosc) → test → WZ FIFO (opcja 7)

---

# DZIEŃ 17 (2026-08-25) — PZ KOMPLET: zapis do bazy + test na żywo ✅

## 1. INSERT do Partie (dziury @)

```csharp
command.CommandText = "INSERT INTO Partie (PrzedmiotId, Ilosc, Cena, Data, Status, BatchNumber) VALUES (@przedmiotId, @ilosc, @cena, @data, @status, @batchNumber)";
command.Parameters.AddWithValue("@przedmiotId", wybraneId);
...
```

- **`@cena` = dziura (placeholder)** — SQL widzi znak, wartość wgrywa `AddWithValue`. Nazwy muszą się zgadzać (`@cena` w SQL = `"@cena"` w kodzie). Po co dziury? Bezpieczeństwo: wpisany tekst nigdy nie dotyka samego SQL-a.
- **`@data` = `DateTime.Today.ToString("yyyy-MM-dd")`** — data dzisiejsza AUTOMATYCZNIE
- **`@status` = `"Przyjete"`** — na sztywno, AUTOMATYCZNIE (umowa: mniej pisania)
- Format daty `yyyy-MM-dd` = **ISO, NIE amerykański** (amerykański to MM/dd/yyyy). Identyczny z API → jedna baza, jeden zapis.

## 2. UPDATE Przedmioty — podbicie stanu

```csharp
command.CommandText = "UPDATE Przedmioty SET Ilosc = Ilosc + @ilosc WHERE Id = @przedmiotId";
```

- `Ilosc = Ilosc + @ilosc` = **nowa ilość = stara z bazy + przyjęta** (dokładamy; opcja 3 nadpisywała całość)
- `WHERE Id = @przedmiotId` — tylko wiersz wybranego artykułu
- Dwie książki: **Partie** = dziennik dostaw (każda dostawa = wiersz), **Przedmioty** = aktualny stan (suma). INSERT pisze do dziennika, UPDATE podbija sumę.

## 3. Test na żywo ✅

- Opcja 6: Wąż ogrodowy, ilość 5, cena 50, partia `TEST-0825` → „✅ Przyjęto dostawę"
- Opcja 2: Wąż ogrodowy **12 → 17** (UPDATE zadziałał)
- Strona **/partie**: wiersz `TEST-0825` (Id 4, Wąż ogrodowy, 5 szt., 50 zł, 2026-08-25, Przyjete) — **konsola → baza → API → strona, jeden łańcuch** 🎉
- Detal: Id 1,3,4 (brak 2) — AUTOINCREMENT nigdy nie używa Id drugi raz po usunięciu

## 4. Lekcja: literówki = „wzrok programisty"

W bloku zapisu było ~10 literówek (`Parametrs`, `AddWythValute`, `ExecuteNonQwery`, `SqlLiteConnection`, dwa `=` zamiast `+`, spacja w `@przedmiot Id`, string rozbity na 2 linie). VS podkreśla je na czerwono. Znaleźć je samemu = dobre ćwiczenie; przy dużej liczbie — śmiało delegować poprawę, to męczarnia, nie nauka.

## Status (koniec dnia 17)

- ✅ **PZ KOMPLETNE** (opcja 6): lista → wybór → dane → INSERT + UPDATE → test na żywo ✅
- ⏭️ **Następne:** **WZ (opcja 7) — logika FIFO** — wydaj od najstarszej partii (ORDER BY Id ASC), wyzerowana → Status="Wydane", za mało towaru → komunikat. Rozmiar: największy pojedynczy kawałek logiki (pętla „zjada" partie po kolei), ale do podziału na 3–4 małe kroki.

---

# DZIEŃ 18 (2026-08-26) — NU1903 naprawione + quiz powtórkowy ✅

## 1. NU1903 — luka w bibliotece SQLite (fix)

- **Diagnoza:** tylko konsola Magazyn miała podatny pakiet — `Microsoft.Data.Sqlite 10.0.10` ciągnie **SQLitePCLRaw 2.1.11** (luka GHSA-2m69-gcr7-jv3q, wysoka). API miało już 10.0.11 → czyste.
- **Łańcuch warstw:** Twój kod → `Microsoft.Data.Sqlite` (to wpisujesz w kodzie) → `SQLitePCLRaw` (łącznik .NET↔C) → `e_sqlite3` (silnik SQLite, napisany w C) → `magazyn.db`
- **Fix:** `dotnet add package Microsoft.Data.Sqlite --version 10.0.11` → SQLitePCLRaw 2.1.12
- **Nowe narzędzie:** `dotnet list package --vulnerable --include-transitive` = skaner podatności (sprawdza też pakiety „ukryte", przechodnie)
- Commit `1f737ea` push, test na żywo OK (lista czyta się normalnie, Wąż 17 na miejscu)

## 2. Quiz powtórkowy z PZ (3/5)

- ✅ UPDATE `Ilosc = Ilosc + @ilosc` — podbicie stanu (stara + przyjęta)
- ✅ Format daty ISO `yyyy-MM-dd`
- 🟡 Lista `idy` = **tłumacz** numerków ekranowych na prawdziwe Id (dziury po usuniętych rekordach to tylko połowa odpowiedzi)
- ❌ `@ilosc` = dziura wypełniana przez `AddWithValue` wartością od użytkownika
- ❌ ExecuteNonQuery vs ExecuteReader

## 3. „List do bazy" — model poleceń SQL ✅ (ZAŁAPANE)

- `CommandText` = **zwykły string** (tekst w języku SQL, który rozumie baza)
- `AddWithValue` = dorzucenie wartości do listu
- `Execute*` = wysłanie listu
- **INSERT/UPDATE/DELETE** = rozkaz → baza robi, NIE odpowiada → `ExecuteNonQuery`
- **SELECT** = pytanie → baza oddaje TABELKĘ → `ExecuteReader` + `while (reader.Read())`
- Zasada-1-zdanie: „czy baza ma coś **oddać**?" → Reader. „Czy ma coś **zrobić**?" → NonQuery.

## Status (koniec dnia 18)

- ✅ **NU1903 naprawione** (10.0.10 → 10.0.11), push, test na żywo ✅
- ⏭️ **Następne:** **WZ (opcja 7) — logika FIFO** — czeka; dziury `@` i UPDATE wracają od razu (powtórka z dnia 17/18)

---

# DZIEŃ 19 (2026-08-27) — WZ FIFO: Krok 1 + 2a ✅ (Krok 2b jutro)

## 1. FIFO — koncept dnia

- **FIFO = First In, First Out** — „kto pierwszy wszedł, pierwszy wychodzi"
- Najstarsza partia schodzi pierwsza → kluczowe SQL: `ORDER BY Id ASC` (Id rośnie z datą dodania)
- Przykład: partie 10/5/7 szt, wydajesz 12 → znika cała najstarsza (10) + 2 z drugiej

## 2. WZ — Krok 1: wybór artykułu + ilość ✅

- Blok `else if (wybor == "7")` — **wzorzec z PZ** (lista artykułów z numerkami, `List<int> idy`, walidacja TryParse, pętla ilości) — 95% powtórka
- Różnice vs PZ: brak ceny i numeru partii (w WZ to baza wybiera partie po FIFO)

## 3. WZ — Krok 2a: pokaz partii w kolejności FIFO ✅

```sql
SELECT Id, Ilosc, BatchNumber FROM Partie
WHERE PrzedmiotId = @przedmiotId AND Ilosc > 0
ORDER BY Id ASC
```

- `AND Ilosc > 0` — pomiń partie puste (już Wydane)
- `ORDER BY Id ASC` — najstarsza pierwsza = serce FIFO
- Test na 2 partiach Węża ✅: `[TEST-0825] 5 szt` / `[TEST-0826] 10 szt` (0826 dodana przez PZ)

## 4. Trim() — spacja w menu ✅

- `string wybor = (Console.ReadLine() ?? "").Trim();`
- `" 7"` (spacja) ≠ `"7"` — program porównuje tekst **znak po znaku**; Trim() zdejmuje spacje z początku/końca (w środku zostają)

## 5. Incydenty dnia

- **Build lock MSB3026/27:** stara instancja Magazyn.exe (drugi terminal) blokuje plik → `taskkill /F /PID <pid>`
- **Przypadkowe usunięcie Grabie:** w menu usuwania numerki = lista przedmiotów (7 = Grabie), nie opcje menu → odzysk opcją 1 (Dodaj); Grabie dostało nowe Id (AUTOINCREMENT nie używa starych)
- Partie NIE mają klucza obcego → usunięcie przedmiotu nie kasuje partii

## Status (koniec dnia 19)

- ✅ Krok 1 + 2a działają (test na żywo), Trim() dodany
- ⏭️ **Jutro: Krok 2b** — pętla wydawania (kod czeka w czacie): `int pozostalo = ilosc;`, `while (reader.Read() && pozostalo > 0)`, partia cała → `Ilosc = 0, Status = 'Wydane'` / część → `Ilosc = Ilosc - @ile`, potem `UPDATE Przedmioty SET Ilosc = Ilosc - @ilosc`
- ⏭️ Test FIFO: wydaj 4 z Węża → Wąż 17→13, TEST-0825 5→1 szt (najstarsza schodzi pierwsza!)

---

# DZIEŃ 20 (2026-08-28) — Zmienne po angielsku (PRO) ✅ + WZ Krok 2a

## 1. Decyzja dnia: kod po angielsku

- **Zmienne w kodzie → angielskie** (standard w pracy zawodowej; nauka angielskiego przez kod)
- **Kolumny bazy zostają polskie** (`Ilosc`, `PrzedmiotId`...) — ta sama baza `magazyn.db` działa z API (MagazynApi) i stroną (MagazynWeb); zmiana nazw zepsułaby oba projekty
- **Teksty na ekranie** (`Console.WriteLine`) też zostają po polsku — to UI dla użytkownika

## 2. Tabela zamiany polskie → angielskie

| Polskie | Angielskie | Znaczenie |
|---|---|---|
| `wybor` | `choice` | wybór z menu |
| `ilosc` / `iloscTekst` | `quantity` / `quantityText` | ilość / tekst ilości |
| `cena` / `cenaTekst` | `price` / `priceText` | cena / tekst ceny |
| `nazwa` / `nazwy` | `name` / `names` | nazwa / nazwy |
| `idy` | `ids` | lista Id |
| `numer` | `number` | licznik na ekranie |
| `indeks` | `index` | indeks na liście |
| `wybraneId` | `selectedId` | wybrane Id |
| `przedmiotId` (@) | `itemId` | Id artykułu |
| `pozostalo` | `remaining` | ile zostało |
| `edytowany` | `editedItem` | edytowany przedmiot |
| `suma` | `total` | suma wartości |
| `klawisz` | `key` | wciśnięty klawisz |
| `pokazanoCos` | `anythingShown` | czy coś pokazano |
| `komendaUsun` | `deleteCommand` | komenda usuwania |
| `nowaNazwa` / `nowaIlosc` / `nowaCena` | `newName` / `newQuantity` / `newPrice` | nowa wartość |
| `@data` | `@date` | dziura daty |

⚠️ Lekcja: przy zamianie uważać na **teksty na ekranie** — `numer` → `number` złapało też „Podaj numer..." i trzeba było cofać (zamiana ma być tylko w zmiennych, nie w UI).

## 3. WZ — Krok 2a (blok wydawania): początek wpisany

- `int remaining = quantity;` — licznik: ile jeszcze trzeba wydać
- `while (reader.Read() && remaining > 0)` — czytaj partie, dopóki coś zostało do wydania
- `partId`, `inBatch` — numer partii i jej stan (nowe, angielskie nazwy)
- `dotnet build` → **powodzenie ✅** (cały plik po przeróbce kompiluje się)

## Status (koniec dnia 20)

- ✅ Zmienne po angielsku (cały Program.cs) — build przechodzi
- ✅ **WZ FIFO — kod KOMPLETNY** (Krok 1 + 2a + 2b):
  - 2b-1: gałąź „cała partia schodzi" — `conn2`, `UPDATE Partie SET Ilosc = 0, Status = 'Wydane' WHERE Id = @partId`, `remaining -= inBatch`
  - 2b-2: gałąź `else` (część partii) — `UPDATE Partie SET Ilosc = Ilosc - @take`, **`remaining = 0`** (⚠️ nie `-= inBatch` — wyszłoby −1!)
  - 2b-3: po pętli — `UPDATE Przedmioty SET Ilosc = Ilosc - @quantity` + komunikat „✅ Wydano" / „❌ Za mało towaru"
- ⚠️ Lekcja dnia: wklejka 2b-3 wpadła **w środek else** (widać po wcięciach) — przeniesiona za blok wydawania
- ⏭️ **JUTRO (Dzień 21): TEST FIFO na żywo** — menu 7 → Wąż → 4 → „Wydaję z [TEST-0825]..." → ✅ → opcja 2: Wąż 13 (17−4) → opcja 7: TEST-0825 = 1 szt / TEST-0826 = 10 szt (4 wzięte z NAJSTARSZEJ!) → ewentualnie test „za mało towaru" + „0 - anuluj" w menu usuwania

## 4. Baza przeniesiona do MagazynBaza (dopisek po sesji)

- **Nowa lokalizacja bazy:** `D:\Dane\Projekty\NaukaCSharp\MagazynBaza\magazyn.db` — dedykowany folder (user: „ładniej by było w jakimś folderze")
- **Czemu:** baza jest wspólna dla konsoli, API i strony → nie powinna siedzieć w folderze żadnego z nich (gdyby Magazyn zniknął, dane zostają)
- **Co się zmieniło:**
  - konsola: `Program.cs` → `string connectionString = "Data Source=D:\\Dane\\Projekty\\NaukaCSharp\\MagazynBaza\\magazyn.db";`
  - API: `MagazynApi\appsettings.json` → `"MagazynDb": "Data Source=D:\\...\\MagazynBaza\\magazyn.db"`
  - strona (MagazynWeb): **nic** — czyta przez API
- **Test na żywo ✅:** opcja 2 pokazała wszystkie przedmioty z nowej lokalizacji (Wąż ogrodowy 27 szt!)
- **Uwaga:** ścieżka z `\\` = w stringu C# backslash musi być podwojony (ucieczka)
- Commity: konsola `b7f8889`, API `3511bda` (push na GitHub)

## DZIEŃ 21 (2026-08-29) — TEST FIFO + PZ "Enter = stara cena"

### 1. TEST FIFO na żywo — BUG znaleziony i naprawiony ✅

- **Pierwsza próba → crash:** `SQLite Error 5: 'database is locked'` na `cmd2.ExecuteNonQuery()` (linia 469)
- **Co to znaczy:** baza zablokowana. `connection` (1.) CZYTAŁ partie (reader otwarty w środku pętli), a `conn2` (2.) chciał PISAĆ UPDATE w tym samym momencie. SQLite na to nie pozwala (jak czytający książkę, której ktoś chce dopisywać).
- **LEKCJA (ważna):** **nie czytać i pisać naraz z dwóch połączeń.** Czytanie musi się SKOŃCZYĆ zanim zacznie się pisanie.
- **Fix (wzorzec parking — znany z opcji 4!):** najpierw wrzuć wszystko do list, potem pisz:
  - 3 listy: `partIds`, `batchSizes`, `batchNumbers` (parking)
  - pierwsza pętla `while (reader.Read())` tylko wrzuca do list (`Add×3`)
  - druga pętla `while (i < partIds.Count && remaining > 0)` chodzi po parkingu (przez `i` + `i++`) i dopiero TAM jest `conn2` z UPDATE
- **Test po fixie ✅:** `Wydaję z [TEST-0825]...` → `✅ Wydano towar` → Wąż ogrodowy **27 → 23** (4 szt z NAJSTARSZEJ partii)

### 2. PZ: "Enter = zostaw starą cenę" (funkcja na życzenie usera)

- Cel: nie chcę podawać ceny co dostawę — Enter ma zostawić starą cenę artykułu
- **3 zmiany (wzorce znane z opcji 3 i 4):**
  - parking cen: `List<decimal> prices` + SELECT z `Cena` + `prices.Add(Convert.ToDecimal(reader["Cena"]))`
  - `decimal oldPrice = prices[index];` (po `selectedId`)
  - `decimal price = oldPrice;` + `if (priceText != "")` wokół walidacji → Enter (puste) = pomijamy pętlę, zostaje stara cena
- komunikat pokazuje cenę: `$"Podaj cenę za sztukę (Enter = {oldPrice} zł):"` — `$` = wstawianie wartości, bez `$` program drukuje `{oldPrice}` dosłownie!
- **Test ✅:** Enter przy cenie → od razu numer partii → `✅ Przyjęto dostawę` → Wąż 23 → 25

### 3. Drobiazgi

- **build lock (2. raz):** stara instancja Magazyn.exe (PID 7668) z innego terminala blokowała exe (MSB3026/27) → `tasklist` + `taskkill /F /PID 7668` → OK
- **warning CS8604 naprawiony:** `Convert.ToString(...)` może zwrócić null → `?? ""` („jeśli null, wstaw pusty tekst")

## Status (koniec dnia 21)

- ✅ TEST FIFO na żywo — ZALICZONY (Wąż 27→23, wydanie z TEST-0825)
- ✅ PZ: Enter = stara cena — ZALICZONY (Wąż 23→25, partia z ceną 45)
- ✅ Błąd "database is locked" naprawiony (parking: czytaj do list, potem pisz)
- ⏭️ NASTĘPNE: „0 - anuluj" w menu usuwania, zaległy quiz (@-dziury, Execute), dalej w API/frontend



