# PokiePaws Desktop

Aplikacja desktopowa dla sieci franczyzowej PokiePaws

### Platforma: C# / .NET 9 / WPF
### Obsługuje role: Pracownik magazynu
### Komunikuje sie z centralnym REST API + WebSocket (real-time)

## Opis
Aplikacja desktopowa jest jednym z czterech komponentów systemu PokiePaws.
Przeznaczona dla pracowników magazynu firmy centralnej — umozliwia zarzadzanie katalogiem produktów, obsluge zamówien od gabinetów weterynaryjnych oraz monitorowanie stanów magazynowych.
Architektura MVVM, powiadomienia w czasie rzeczywistym, tryb offline z lokalna baza SQLite.

## Funkcjonalnosci

### Logowanie (email + haslo) - dla Pracownika magazynu

### Panel pracownika magazynu
- Dashboard z biezacymi zamówieniami od gabinetów weterynaryjnych
- Przegladanie i filtrowanie zamówien po statusie i gabinecie
- Zmiana statusu zamówienia (nowe - w realizacji - wyslane - dostarczone)
- Zarzadzanie katalogiem produktów (CRUD, stany magazynowe)
- Alerty o niskim stanie magazynowym produktu
- Lista gabinetów sieci i historia ich zamówien
- Powiadomienia real-time o nowych zamówieniach (WebSocket)

### Powiadomienia real-time (WebSocket) - nowe zamówienia od gabinetów
### Obsluga jezyka polskiego i angielskiego
### Tryb offline - dane dostepne bez polaczenia, synchronizacja po przywróceniu sieci
### Spojna identyfikacja wizualna PokiePaws we wszystkich warstwach

## Stos technologiczny

| Warstwa | Technologia |
|---|---|
| Jezyk | C# |
| Framework | .NET 9 |
| UI | WPF (XAML) |
| Architektura | MVVM - CommunityToolkit.Mvvm |
| Siec / API | HttpClient + RestSharp (REST, JWT) |
| Real-time | WebSocket |
| Baza lokalna | SQLite + EF Core (cache offline) |
| Autoryzacja | Windows Credential Manager (DPAPI) |
| Testy | xUnit + Moq |
| Dystrybucja | dotnet publish self-contained (.exe, win-x64) |

## Uruchomienie

git clone https://github.com/PokiePaws/pokiepaws-desktop.git
cd pokiepaws-desktop
dotnet run --project PokiePawsDesk

## Powiazane repozytoria

| Komponent | Repozytorium |
|---|---|
| API (Spring Boot) | https://github.com/PokiePaws/pokiepaws-api |
| Aplikacja webowa (Next.js) | https://github.com/PokiePaws/pokiepaws-web |
| Aplikacja mobilna (Android) | https://github.com/PokiePaws/pokiepaws-mobile |
