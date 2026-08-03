# Nauka C# — Magazyn, dzień 3 (29.07.2026)

## 1. Interpolacja stringa — `$"..."`

```csharp
Console.WriteLine($"Podaj nową nazwę (Enter = zostaw {edytowany.Name}):");
```

- `$` przed cudzysłowem mówi: "będę wstawiać zmienne w środek tekstu"
- `{zmienna}` — w to miejsce wkleja się wartość zmiennej
- Bez `$` musiałbyś sklejać plusikami: `"tekst " + zmienna + " dalej"`
- Program **nie rozumie słowa "zostaw"** — to tylko litery wyświetlone na ekranie dla Ciebie

## 2. `=` vs `==` vs `!=`

| Znaczek | Znaczenie | Przykład |
|---------|-----------|----------|
| `=` | **wpisywanie** do zmiennej | `nowaNazwa = "Mydło";` |
| `==` | **pytanie "czy równe?"** | `if (nowaNazwa == "")` |
| `!=` | **pytanie "czy NIE równe?"** | `if (nowaNazwa != "")` |

- `!=` czytaj jako **JEDEN znaczek** = **"różne"** / "NIE równe"
- Wykrzyknik `!` zawsze znaczy zaprzeczenie (NIE):
  - `!=` — NIE równe
  - `!true` — NIE prawda (czyli fałsz)

## 3. Zasięg zmiennej (scope)

- Zmienna istnieje **tylko wewnątrz klamerek `{ }`** w których została stworzona
- Przykład: `Item edytowany = magazyn[indeks];` jest w `else { ... }` — istnieje tylko tam
- Kod poza else'm nie może użyć `edytowany` — kompilacja by padła

```
else
{
    Item edytowany = magazyn[indeks];   // żyje tylko tutaj
    // można używać edytowany
}
// tutaj edytowany już nie istnieje!
```

## Kontekst: co robiły te linijki

Dodawanie edycji nazwy przedmiotu w opcji "4 - Edytuj przedmiot":

```csharp
Console.WriteLine($"Podaj nową nazwę (Enter = zostaw {edytowany.Name}):");
string nowaNazwa = Console.ReadLine() ?? "";
if (nowaNazwa != "")
{
    edytowany.Name = nowaNazwa;
}
```

- Pyta o nową nazwę, pokazując starą w nawiasie
- Jeśli wpiszesz coś → zmienia nazwę
- Jeśli naciśniesz Enter (nic nie wpiszesz) → zostawia starą
