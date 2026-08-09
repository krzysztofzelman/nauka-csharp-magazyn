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
- 🚧 Usuwanie: krok 2 (DELETE) — czeka na koniec
- ⏳ Wartość magazynu → do migracji (na razie liczy z listy)
