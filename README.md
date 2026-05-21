# PokiePaws Desktop

Repozytorium: https://github.com/PokiePaws/pokiepaws-desktop

Aplikacja desktopowa WPF jest przeznaczona dla pracowników magazynu firmy centralnej PokiePaws. Służy do obsługi zamówień gabinetów, zarządzania katalogiem produktów, kontrolowania stanów magazynowych oraz odbierania powiadomień real-time o nowych zamówieniach.

## Rola aplikacji

Desktop obsługuje rolę Pracownik magazynu. Aplikacja komunikuje się z centralnym PokiePaws API przez REST oraz WebSocket Secure.

## Stos technologiczny

- C#
- .NET 9
- WPF / XAML
- MVVM
- CommunityToolkit.Mvvm
- HttpClient + RestSharp
- WebSocket
- SQLite + EF Core
- Windows Credential Manager
- xUnit
- Moq
- Dystrybucja przez `dotnet publish --self-contained`
- Docelowy runtime: `win-x64`

## Funkcjonalności do wykonania

### Uwierzytelnianie

- Logowanie pracownika magazynu.
- Obsługa tokenów JWT.
- Automatyczne dołączanie tokenu do żądań API.
- Obsługa wylogowania.
- Usuwanie tokenów i cache po wylogowaniu.

### Dashboard magazynu

- Widok bieżących zamówień od gabinetów.
- Liczba nowych zamówień.
- Liczba zamówień w realizacji.
- Alerty niskich stanów magazynowych.
- Szybki podgląd najważniejszych zamówień.

### Zamówienia

- Lista zamówień od gabinetów.
- Filtrowanie zamówień po statusie.
- Filtrowanie zamówień po gabinecie.
- Szczegóły zamówienia.
- Zmiana statusu zamówienia według procesu:
  - nowe
  - w realizacji
  - wysłane
  - dostarczone
- Historia zmian statusu, jeżeli udostępnia ją API.

### Produkty i magazyn

- Lista produktów firmy centralnej.
- Dodawanie produktu.
- Edycja produktu.
- Usuwanie lub dezaktywacja produktu.
- Zarządzanie stanami magazynowymi.
- Alert niskiego stanu magazynowego.
- Oznaczanie produktów wymagających uzupełnienia.

### Gabinety

- Lista gabinetów sieci PokiePaws.
- Historia zamówień danego gabinetu.
- Podgląd danych potrzebnych do realizacji dostawy.

### Powiadomienia real-time

WebSocket musi obsługiwać:

- Powiadomienie o nowym zamówieniu od gabinetu.
- Aktualizację listy zamówień po otrzymaniu powiadomienia.
- Czytelny komunikat w UI bez konieczności ręcznego odświeżania.

### Tryb offline

- Wyświetlanie ostatnio zsynchronizowanych danych w trybie offline.
- Lokalny cache zamówień, produktów i gabinetów.
- Synchronizacja danych po przywróceniu połączenia.
- Kolejkowanie bezpiecznych operacji, jeżeli zostanie uzgodnione z API.
- Informowanie użytkownika, które dane mogą być nieaktualne.

### Wielojęzyczność

- Interfejs w języku polskim i angielskim.
- Teksty w zasobach aplikacji, bez hardcodowania w widokach.

## Bezpieczeństwo

### Przechowywanie tokenów

- Tokeny JWT przechowywane w Windows Credential Manager.
- Ochrona przez DPAPI, czyli Data Protection API.
- Tokeny szyfrowane kluczem powiązanym z kontem Windows aktualnego użytkownika.
- Hasła nie mogą być przechowywane w plikach konfiguracyjnych.
- Hasła nie mogą być przechowywane w zmiennych środowiskowych.

### Komunikacja z API

- Komunikacja REST wyłącznie przez HTTPS z TLS 1.3.
- Walidacja certyfikatu serwera wymuszana przez HttpClient.
- Brak możliwości wyłączenia walidacji certyfikatu w buildzie produkcyjnym.
- WebSocket wyłącznie przez WSS.
- Token JWT dołączany w handshake WebSocket.
- Timeout połączeń sieciowych: 30 sekund.
- Ponowne próby połączenia z wykładniczym backoff.

### Dane lokalne

- Lokalna baza SQLite szyfrowana przez SQLCipher.
- Klucz szyfrowania powiązany z poświadczeniami systemowymi użytkownika.
- Cache usuwany po wylogowaniu lub wygaśnięciu sesji.
- Dane offline muszą być ograniczone do informacji potrzebnych pracownikowi magazynu.

### Logi

Logi aplikacji nie mogą zawierać:

- Tokenów.
- Haseł.
- Danych osobowych.
- Danych medycznych.
- Szczegółów, które mogłyby ujawnić sekrety API.

Poziom logowania w buildzie produkcyjnym powinien być ograniczony do `WARNING` i `ERROR`.

## Architektura aplikacji

Zalecany podział:

- `Presentation`: widoki WPF, XAML, konwertery, style.
- `ViewModels`: logika prezentacji zgodna z MVVM.
- `Domain`: modele domenowe, walidacja, przypadki użycia.
- `Data`: klient API, DTO, EF Core, SQLite, repozytoria.
- `Security`: Credential Manager, obsługa tokenów, czyszczenie sesji.
- `Realtime`: obsługa WebSocket i powiadomień.
- `Sync`: cache offline i synchronizacja.

## Minimalne widoki

- Logowanie.
- Dashboard magazynu.
- Lista zamówień.
- Szczegóły zamówienia.
- Zmiana statusu zamówienia.
- Lista produktów.
- Formularz dodawania i edycji produktu.
- Stany magazynowe.
- Alerty niskich stanów.
- Lista gabinetów.
- Historia zamówień gabinetu.
- Ustawienia i wylogowanie.

## Kryteria ukończenia

- Pracownik magazynu może się zalogować i wylogować.
- Dashboard pokazuje bieżące zamówienia.
- Zamówienia można filtrować po statusie i gabinecie.
- Status zamówienia można zmienić zgodnie z wymaganym procesem.
- Katalog produktów obsługuje CRUD i stany magazynowe.
- Niski stan magazynowy jest widoczny w aplikacji.
- WebSocket informuje o nowym zamówieniu.
- Dane są dostępne offline w zakresie cache.
- Synchronizacja działa po odzyskaniu połączenia.
- Tokeny są zapisane w Windows Credential Manager.
- Lokalna baza jest szyfrowana.
- Logi produkcyjne nie zawierają danych wrażliwych.
- Aplikacja ma wersję PL i EN.
- Testy jednostkowe obejmują ViewModel, serwisy, klienta API i logikę synchronizacji.
- Aplikacja publikuje się jako self-contained `.exe` dla `win-x64`.
