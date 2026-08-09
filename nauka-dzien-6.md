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

1. **Opcja 3 (Edycja) → baza:** SELECT przedmiotów (jak opcja 2) + parking `idy` (wzorzec z opcji 4) → `UPDATE Przedmioty SET Nazwa=@nazwa, Ilosc=@ilosc, Cena=@cena WHERE Id = @id`
   - Do decyzji: edytor z backspace — rozszerzyć na ilość/cenę, czy ujednolicić do ReadLine?
2. **Opcja 5 (Wartość) → baza:** suma z bazy — pętla (jak opcja 2) albo `SELECT SUM(Ilosc * Cena)` (jedno pytanie, zero pętli)
3. **Sprzątanie:** usunąć `List<Item> magazyn`; los `Funkcje.WyswietlListe` / `WartoscMagazynu` / `Item.cs` do decyzji
4. **Opcja 4, krok 2:** `DELETE FROM Przedmioty WHERE Id = @id` — nowy koncept: **WHERE** ("gdzie" — usuń TYLKO ten wiersz)

### Status

- ✅ Dodawanie → baza (INSERT)
- ✅ Wyświetlanie → baza (SELECT)
- ✅ Usuwanie: krok 1 (lista z bazy + parkingi Id)
- 🚧 Usuwanie: krok 2 — **wstrzymany** do czasu naprawy dwuźródłowości
- ⏳ Edycja → do migracji — dopóki nie zrobiona, **nie testować edycji** (zmiany niewidoczne przy wyświetlaniu z bazy)
- ⏳ Wartość magazynu → do migracji (na razie liczy z listy w pamięci)
