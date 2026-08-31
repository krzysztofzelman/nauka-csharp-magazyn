# DRZEWO PLIKÓW — co gdzie leży i co robi

> Ściąga do notatek `mapa-projektu.md`. Tu patrzysz, GDZIE co jest; tam — JAK to działa (piosenki).

```
NaukaCSharp/
├── Magazyn/          ← KONSOLA (stara kasa — menu 1-8)
├── MagazynApi/       ← API (kuchnia — serwuje dane, port 5109)
├── MagazynWeb/       ← STRONA (sala — pokazuje i zbiera kliknięcia, port 5008)
├── MagazynShared/    ← WSPÓLNA PÓŁKA (modele dla API i strony — NIE ruszaj bez potrzeby)
└── MagazynBaza/      ← BAZA (jeden plik magazyn.db — cały magazyn)
```

---

## 📁 Magazyn/ — konsola (stara kasa)

| Plik | Co robi |
|------|---------|
| `Program.cs` | CAŁY program: menu 1–8 (CRUD, 6 = PZ przyjmij, 7 = WZ wydaj FIFO). Tu uczysz się piosenki SQLite |
| `Item.cs` | Przepis na przedmiot: Nazwa, Ilość, Cena |
| `nauka-dzien-3/4/5/6.md` | Dziennik nauki — sekcje „Status (koniec dnia X)" mówią, gdzie jesteśmy |
| `mapa-projektu.md` | Ściąga 1-stronicowa: 3 warstwy, piosenki, tabela błędów |
| `rozmowa-2026-08-09.md` | Zapis rozmowy (notatki) |

## 📁 MagazynApi/ — API (kuchnia)

| Plik | Co robi |
|------|---------|
| `Program.cs` | Start API, konfiguracja, połączenie z bazą (czyta connection string z appsettings) |
| `Controllers/BatchController.cs` | Obsługa **partii**: GET/POST/PUT/DELETE (`/api/Batch`) + **wydanie WZ** (`/api/Batch/wydanie` — logika FIFO, zmniejsza stan) |
| `Controllers/PrzedmiotyController.cs` | Obsługa **przedmiotów**: CRUD (`/api/przedmioty`) |
| `appsettings.json` | Konfiguracja — KLUCZ: `MagazynDb` = ścieżka do pliku bazy |

## 📁 MagazynShared/ — wspólna półka (modele)

| Plik | Co robi |
|------|---------|
| `Batch.cs` | Przepis na partię: Id, BatchNumber, ItemId, Quantity, Price, Date, Status, Nazwa |
| `Item.cs` | Przepis na przedmiot: Id, Name, Quantity, Price |
| `DispatchRequest.cs` | Przepis na żądanie wydania: ItemId, Quantity — to, co wysyła przycisk Wydaj |

API i strona używają TYCH SAMYCH przepisów — dlatego JSON się zgadza.

## 📁 MagazynWeb/ — strona (sala)

| Plik | Co robi |
|------|---------|
| `Program.cs` | Start strony |
| `Lang.cs` | Tłumacz PL/EN: 2 słowniki + `T()` (pytaj), `Toggle()` (przełącz), `Set()` (ustaw) |
| `Components/App.razor` | Korzeń aplikacji — pierwszy komponent, łączy wszystko |
| `Components/Routes.razor` | Mapa tras — który komponent odpowiada za który adres |
| `Components/_Imports.razor` | Wspólne usingi (żeby nie pisać ich w każdym pliku) |
| `Components/Layout/MainLayout.razor` | Szkielet strony: pasek u góry (navbar) + miejsce na treść |
| `Components/Layout/LangSwitch.razor` | Przycisk PL\|EN („wyspa interaktywna") |
| `Components/Layout/ReconnectModal.razor` | Okienko „łączenie z serwerem..." (szablon VS) |
| `Components/Pages/Home.razor` | Strona przedmiotów `/`: tabela + formularz + szukajka + sumy |
| `Components/Pages/Batches.razor` | Strona partii `/partie`: tabela + formularz + Wydaj (Dispatch) + Anuluj |
| `Components/Pages/Error.razor` | Strona błędu (nie dotykaj) |
| `Components/Pages/NotFound.razor` | Strona 404 (nie dotykaj) |
| `appsettings.json` | Konfiguracja strony |
| `Properties/launchSettings.json` | Profile startowe — porty (strona = 5008) |

## 📁 MagazynBaza/ — magazyn danych

| Plik | Co robi |
|------|---------|
| `magazyn.db` | **JEDEN plik bazy SQLite** dla konsoli, API i strony. Tabele: `Przedmioty` (stan) + `Partie` (dziennik dostaw) |

---

## Jak się to łączy (1 obrazek)

```
Konsola ──┐
API ──────┼──► MagazynBaza\magazyn.db   (wszyscy czytają/piszą TĘ SAMĄ bazę)
Strona ───┘

Strona ──HTTP──► API ──SQL──► baza     (strona NIE dotyka bazy bezpośrednio!)
API + Strona ──► MagazynShared         (modele: wspólne przepisy)
```

## Czego NIE dotykać

- `bin/`, `obj/` — śmieci budowania (VS tworzy je sam)
- `.vs/`, `.git/` — ustawienia VS i Gita
- `Error.razor`, `NotFound.razor`, `ReconnectModal.razor` — szablon VS (przydatne, ale nie nasze)
- `MagazynShared/` — bez wyraźnej potrzeby (zmiana modelu = zmiana wszędzie)
