# Słowniczek Batches.razor (EN → PL)

Ściąga do czytania strony `/partie`. Gdy nieznane słowo zablokuje czytanie → zerknij tutaj.
Słowa oznaczone ✅ już znasz (używałeś(-aś) ich bez tłumaczenia) — pomijaj.

> Plik: `MagazynWeb\Components\Pages\Batches.razor` (218 linii). Mapa pięter w notatkach (Dzień 26).

## Piętro 0 — Dach
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `@page "/partie"` | adres strony w przeglądarce (etykieta) |
| `@rendermode InteractiveServer` | tryb strony — przyciski działają (server = kod liczy się na serwerze) |
| `@inject` | „wstrzyknij" — Blazor sam tworzy i podaje gotowy egzemplarz |
| `HttpClient Http` | wysłannik strony — zmienna `Http` umie pytać API |

## Piętro 1 — Tytuł + błąd
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `PageTitle` | tytuł KARTY przeglądarki (zakładki) |
| `h1` | nagłówek na stronie (heading 1 = największy) |
| `dispatchError` | zmienna tekstowa: „błąd wydania" — pusta = brak błędu |
| `alert alert-danger` | czerwona ramka ostrzeżenia (lakier) |
| `@Lang.T("...")` | metoda T z klasy Lang — tłumaczy napis na PL/EN |

## Piętro 2 — Formularz (6 pól)
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `input` | pole do wpisania |
| `@bind` | „zepnij" — łączy pole z zmienną (wpisane → zmienna) |
| `placeholder` | podpowiedź w polu, znika przy pisaniu |
| `batchNumber` | numer partii (np. KOSIARKA-0822) |
| `itemId` | id artykułu (z tabeli artykułów) |
| `quantity` | ilość (quantity = ile sztuk) |
| `price` | cena |
| `date` | data |
| `status` | status (np. Przyjęta) |

## Piętro 3 — Przyciski (l.36–44)
| Słowo | Po polsku (rola w pliku) |
|---|---|
| ✅ `Add` | dodaj (nową partię) |
| ✅ `Save` | zapisz (zmiany edytowanej) |
| ✅ `Cancel` | anuluj (wyjdź z edycji) |
| `editedId` | **karteczka**: numer partii, którą właśnie poprawiamy; 0 = nikogo |
| `@onclick` | „po kliknięciu" — co ma się wykonać |

## Piętro 4 — Tabela
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `batches` | zmienna: lista partii pobrana z API |
| `null` | pusto / nic (jeszcze nie ma danych) |
| `Loading` | ładowanie… |
| `table / thead / tbody` | tabela / nagłówek tabeli / ciało (wiersze) |
| `tr / th / td` | wiersz / komórka nagłówka / zwykła komórka |
| `@foreach (var batch in batches)` | dla każdej partii z listy narysuj wiersz |
| `var` | „niech C# sam zgadnie typ" |
| `batch` | zmienna „aktualna partia" (co obieg inna) |
| ✅ `Edit` | edytuj (przycisk w wierszu) |
| ✅ `Delete` | usuń (przycisk w wierszu) |
| `Dispatch` | **wydaj** (przycisk w wierszu — wydanie towaru) |
| `Actions` | akcje (nagłówek kolumny z przyciskami) |
| `BatchNumber / Name / Price / Date / Status` | pola partii (numer/nazwa artykułu/cena/data/status) |

## Piętro 5 — Schowek `@code` (zmienne)
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `@code` | schowek: tu mieszkają zmienne i metody strony |
| `private` | „prywatne" — tylko dla tej strony |
| `int / string / decimal / bool` | typy: liczba / tekst / cena / prawda-fałsz |
| `List<Batch>` | lista partii (Batch = klasa-przepis, 7 pól) |
| `?` (przy typie) | „może być nic" (null) |
| `Dictionary<int,int> dispatchQuantity` | słownik „ile wydać z partii nr X" (klucz → wartość) |

## Piętro 6 — Metody (schowek, od l.110)
| Słowo | Po polsku (rola w pliku) |
|---|---|
| `async Task` | metoda, która może CZEKAĆ na odpowiedź (nie zamraża strony) |
| `await` | „czekaj na wynik, zanim pójdziesz dalej" |
| `OnInitializedAsync` | odpala się RAZ — gdy strona się otwiera (on = na, initialized = zainicjowana) |
| `Refresh` | **odśwież**: pobierz całą listę od nowa (l.184) |
| `GetFromJsonAsync` | pytanie GET + od razu zamień JSON na obiekty (l.186) |
| `PostAsJsonAsync` | wyślij NOWY obiekt (dodanie, l.133) |
| `PutAsJsonAsync` | wyślij NADPISANIE obiektu (zapis zmian, l.146) |
| `DeleteAsync` | usuń (l.154) |
| `response` | odpowiedź API (status: 200 = ok, 400 = zła prośba…) |
| `StatusCode` | numer statusu odpowiedzi |
| `BadRequest` | 400 — „zła prośba" (np. za mało towaru) |
| `IsSuccessStatusCode` | „czy odpowiedź to sukces?" (prawda/fałsz) |
| `return` | zakończ metodę NATYCHMIAST |
| `api/Batch/wydanie` | adres wydania (wydanie po polsku, bo tak nazwany endpoint) |

## Najczęstsze słowa HTML/lakier (bez nauki)
`div` = pudełko; `class` = styl; `card / btn / form-control / table-striped / row / col-md-*` = gotowe style Bootstrap — profesjonaliści też je kopiują.
