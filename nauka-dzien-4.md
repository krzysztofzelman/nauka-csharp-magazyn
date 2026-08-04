# Nauka C# — Magazyn, dzień 4 (04.08.2026)

## 1. Usuwanie przedmiotu — `RemoveAt`

```csharp
indeks--;                                        // 1. odejmij 1 (lista liczy od zera)
Item usuwany = magazyn[indeks];                  // 2. weź przedmiot "na bok"
magazyn.RemoveAt(indeks);                        // 3. usuń go z listy
Console.WriteLine($"✅ Usunięto przedmiot: {usuwany.Name}");  // 4. pokaż co usunąłeś
```

- `RemoveAt(numer)` — „usuń z listy przedmiot spod numeru"
- Ekran pokazuje 1, 2, 3…, a lista liczy od **zera** — dlatego `indeks--`
- Ważny trik: najpierw **bierzemy przedmiot na bok**, potem usuwamy — inaczej nie mielibyśmy jak pokazać jego nazwy

## 2. „Fizyczna" edycja nazwy — czytanie po jednym klawiszu

Zamiast `ReadLine()` (cała linia naraz) czytamy **JEDEN klawisz** na raz i sami składamy tekst:

```csharp
Console.Write(edytowany.Name);                                   // wypisz starą nazwę (bez nowej linii!)
string nowaNazwa = edytowany.Name;                               // startujemy od starej nazwy
while (true)
{
    ConsoleKeyInfo klawisz = Console.ReadKey(true);              // czekaj na jeden klawisz
    if (klawisz.Key == ConsoleKey.Enter)                         // Enter?
    {
        break;                                                   // koniec pisania
    }
    else if (klawisz.Key == ConsoleKey.Backspace && nowaNazwa.Length > 0)  // Backspace?
    {
        nowaNazwa = nowaNazwa.Substring(0, nowaNazwa.Length - 1);  // obetnij ostatnią literę
        Console.Write("\b \b");                                  // wymaż ją z ekranu
    }
    else if (klawisz.KeyChar != '\0')                            // zwykła litera/cyfra?
    {
        nowaNazwa += klawisz.KeyChar;                            // doklej do nazwy
        Console.Write(klawisz.KeyChar);                          // pokaż na ekranie
    }
}
```

### Nowe pojęcia

| Pojęcie | Co robi |
|---------|---------|
| `Console.ReadKey(true)` | czeka na **jeden klawisz**; `true` = nie pokazuj go na ekranie |
| `ConsoleKeyInfo` | pudełko z informacją o wciśniętym klawiszu |
| `klawisz.Key` | **który** klawisz (Enter, Backspace, …) |
| `ConsoleKey.Enter` / `ConsoleKey.Backspace` | nazwy klawiszy |
| `klawisz.KeyChar` | **jaka litera** (dla zwykłych klawiszy) |
| `'\0'` | pusty znak — Enter i Backspace go mają zamiast litery |
| `nowaNazwa.Length` | ile liter ma tekst |
| `Substring(0, Length - 1)` | wytnij kawałek **bez ostatniej litery** |
| `"\b \b"` | cofnij kursor, nadpisz spacją, wróć — kasowanie z ekranu |
| `Console.Write` | wypisz **bez** przejścia do nowej linii (WriteLine przechodzi) |

### Najważniejsza myśl: pamięć i ekran idą w parze

- dopisujesz literę → **dwie** linie: `nowaNazwa += ...` (pamięć) i `Console.Write(...)` (ekran)
- kasujesz → **dwie** linie: `Substring` (pamięć) i `"\b \b"` (ekran)
- klawisze bez litery (Enter, Backspace, strzałki) mają `KeyChar == '\0'` — dlatego je odfiltrowujemy

## 3. `\0` i `\u0000` — to samo

- `'\0'` — krótka pisownia „pustego znaku" (numer 0 w tabeli znaków)
- `'\u0000'` — ta sama rzecz, zapisana wprost (Unicode 0000)
- Programiści na co dzień piszą `'\0'`

## Plan na następną sesję (weekend)

- **Baza danych SQLite** — przedmioty przetrwają zamknięcie programu
- Baza to lokalny plik `magazyn.db` — bez serwera, bez VPS
- Każda opcja menu dostanie swój rozkaz SQL:
  Dodaj = `INSERT`, Wyświetl = `SELECT`, Edytuj = `UPDATE`, Usuń = `DELETE`
