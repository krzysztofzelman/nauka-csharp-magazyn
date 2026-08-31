# MAPA PROJEKTU — Magazyn

> Jedna baza, trzy fronty. Kod czyta się PO WZORACH (piosenkach), nie po słowach.
> Nie znasz pliku? Szukaj, KTÓRA piosenka w nim gra.

```
      KONSOLA                      API (kuchnia)                 STRONA (sala)
  Magazyn — stara kasa        MagazynApi — port 5109        MagazynWeb — port 5008

  Program.cs                  Program.cs (start)            Home.razor      → przedmioty (/)
  menu: 1-5 CRUD              Controllers\                  Batches.razor   → partie (/partie)
        6 = PZ (przyjmij)     BatchController.cs  → partie  Lang.cs         → tłumacz PL/EN
        7 = WZ (wydaj FIFO)   PrzedmiotyController.cs       LangSwitch.razor → przycisk PL|EN
        8 = wyjście                → przedmioty             MainLayout.razor → pasek u góry

          │                            │                            │
          └──────────┬─────────────────┴─────────────┬──────────────┘
                     ▼                               ▼
              JEDNA BAZA (SQLite):  Magazyn\magazyn.db
              ┌─────────────────────────────────────────────┐
              │ Przedmioty = stan:  co jest TERAZ (1 wiersz/artykuł) │
              │ Partie     = dziennik: skąd się wzięło (1 wiersz/dostawa) │
              └─────────────────────────────────────────────┘
```

## Gdzie czego szukać (1 linijka)

| Chcę zobaczyć…            | Otwieram…                 | Szukam…            |
|---------------------------|---------------------------|--------------------|
| dodawanie przedmiotu      | konsola opcja 1 / PrzedmiotyController / `Dodaj()` w Home.razor | `INSERT` / `POST` / `Dodaj` |
| dodawanie partii          | Batches.razor `Dodaj()`   | `POST /api/Batch`  |
| wydanie towaru (WZ)       | Batches.razor `Dispatch()` | `POST /api/Batch/wydanie` |
| logika FIFO               | BatchController.cs `Wydanie` | `ORDER BY Id ASC` |
| tłumaczenie przycisku     | Lang.cs                   | klucz np. `"Dispatch"` |
| stan magazynu             | Home.razor / `/api/przedmioty` | tabela przedmiotów |

## Piosenki (wzorce — ucz się TYCH, nie słów)

**1. SQL (wszędzie, gdzie jest baza):**
```
using → Open → CreateCommand → CommandText ("list do bazy") → Execute
INSERT/UPDATE/DELETE = rozkaz  → ExecuteNonQuery (baza nic nie odpowiada)
SELECT                = pytanie → ExecuteReader + while(reader.Read()) (baza oddaje tabelkę)
```

**2. Strona (Blazor):**
```
@inject HttpClient → GetFromJsonAsync (pobierz z API) → Lista → @foreach (tabela)
formularz: @bind (pole ↔ zmienna) → @onclick (przycisk → metoda) → POST/PUT → Refresh()
```

**3. HTTP = opcje konsoli:**
```
GET (opcja 2)  = pokaż    POST (opcja 1) = dodaj    PUT (opcja 3) = edytuj    DELETE (opcja 4) = usuń
```

## Jak czytać plik .razor

```
GÓRA pliku = WYGLĄD (HTML):  @page / @rendermode / @inject / tytuł / formularz / tabela
DÓŁ pliku  = LOGIKA (@code): zmienne → OnInitializedAsync → metody (Dodaj, Usun, Edytuj, Dispatch)
@if = pytanie ("jeśli..."), @foreach = powtórz dla każdego wiersza, @bind = połącz pole ze zmienną
```

## Gdzie szukać, gdy coś nie działa

| Objaw                        | Przyczyna            | Fix |
|------------------------------|----------------------|-----|
| czerwone w VS                | kompilator: plik + linia (kliknij 2× na błąd) | popraw |
| strona pusta / „Loading"      | API nie działa       | `netstat -ano \| findstr :5109` → odpal `dotnet run` w MagazynApi |
| „nie ma zmian" po zmianie kodu | stary proces / stara karta | Ctrl+F5 (przeglądarka) + restart |
| „Nazwa X nie istnieje"       | brakuje deklaracji (np. pole `status`) | dodaj linię pola w @code |

## Skróty dnia

- **Ctrl+F5** = startuj wszystko (API + strona), **Ctrl+S** = zapisz, **Ctrl+Z** = cofnij
- Przełącznik PL|EN w pasku u góry; trasy: strona = `/partie`, API = `/api/Batch`, `/api/przedmioty`
- Warnings (żółte CS860x) = „lód na jezdni" — nie blokują, zostawiamy
