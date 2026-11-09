# ParkSpot

Programų sistemų projektavimo kursinio darbo II etapas — veikiantis parkavimo vietų rezervavimo prototipas. I etapo aprašymas: [docs/phase-1.md](docs/phase-1.md).

Vairuotojas rezervuoja vietą bet kurios organizacijos aikštelėje. Administratorius valdo tik savo organizacijos aikšteles, vietas ir rezervacijas. Sistema naudoja vietinę **SQLite** duomenų bazę: pirmą kartą paleista programa pati sukuria `parkspot.db` ir lenteles, o vėliau naudoja išsaugotus duomenis.

## Reikalingi įrankiai

- .NET SDK 10.0 (projektas tikrintas su 10.0.402).
- Pasirinktinai — .NET 10 palaikanti Visual Studio.

MySQL serverio, jo slaptažodžio, PowerShell skriptų, Docker, Node.js ir API raktų nereikia. Pirmą kartą atkuriant NuGet paketus reikia interneto arba jau atsisiųstų paketų.

Technologijos: ASP.NET Core 10, Dapper 2.1.89, MediatR 12.5.0, Microsoft.Data.Sqlite 10.0.9, SQLitePCLRaw.bundle_e_sqlite3 3.0.5 ir xUnit. SQLite biblioteka atsisiunčiama kartu su .NET paketais; atskirai jos diegti nereikia. Tikslios versijos nurodytos `.csproj` failuose. Entity Framework nenaudojamas.

## Paleidimas

### Per Visual Studio

1. Atsisiųskite projektą ir atidarykite `ParkSpot.slnx`.
2. Pasirinkite `ParkSpot.Web` kaip **Startup Project** ir profilį **http**.
3. Paleiskite su **F5** arba **Ctrl + F5**. Visual Studio atkurs reikalingus paketus.
4. Programa automatiškai paruoš duomenų bazę. Susikurkite paskyras pagal tolesnę instrukciją.

### Per terminalą

Komandas vykdykite `ParkSpot` aplanke, kuriame yra `ParkSpot.slnx`. Tinka ir Command Prompt — PowerShell nereikalingas.

```text
dotnet run --project src/ParkSpot.Web --launch-profile http
```

Tai viena paleidimo komanda: prireikus atkuriami paketai, sukompiliuojamas projektas ir sukuriama bazė. Programai sustabdyti spauskite `Ctrl+C`; papildomo DB serverio stabdyti nereikia.

Jei prievadas užimtas arba kompiliuojant užrakintas `ParkSpot.Web.exe`, sustabdykite ankstesnę programos kopiją. Vienu metu jos nepaleiskite ir terminale, ir per Visual Studio tuo pačiu prievadu.

## Duomenų bazė ir konfigūracija

Numatytoji bazės vieta paleidžiant iš projekto:

```text
src/ParkSpot.Web/parkspot.db
```

`Database:Path` nustatymas yra `appsettings.json`. Santykinis kelias skaičiuojamas nuo programos turinio katalogo (`ContentRootPath`), o ne nuo savavališko terminalo aplanko. Publikuotą programą paleiskite iš jos katalogo arba nurodykite absoliutų DB kelią. Programai reikia rašymo teisių bazės aplanke.

Pasirinktinis `appsettings.Local.json` gali pakeisti kelią; jo kurti įprastam paleidimui nereikia. Pavyzdys: [appsettings.Local.example.json](src/ParkSpot.Web/appsettings.Local.example.json).

```json
{
  "Database": {
    "Path": "parkspot.db"
  }
}
```

Galima naudoti ir aplinkos kintamąjį `Database__Path`, turintį pirmenybę prieš JSON. DB naudotojo ir slaptažodžio nėra.

Paskyros, organizacijos, aikštelės ir rezervacijos išlieka po programos perkrovimo. Automatinio demonstracinių duomenų užpildymo nėra. Atsarginei kopijai sustabdykite programą ir nukopijuokite `.db` failą.

## Paskyros ir pirmi duomenys

1. Administratoriaus puslapyje išskleiskite **„Neturi organizacijos paskyros? Registruokis“**.
2. Įveskite organizacijos pavadinimą, vardą, pavardę, el. paštą ir 12–128 simbolių slaptažodį.
3. Prisijunkite sukurta administratoriaus paskyra, sukurkite aikštelę ir pridėkite vietų pagal kiekį.
4. Vairuotojo puslapyje užregistruokite vairuotoją, prisijunkite ir rezervuokite vietą.

Kiekviena administratoriaus registracija sukuria naują organizaciją. Organizacija ir administratorius įrašomi viena transakcija. Organizacijos ID parenka serveris; per registraciją prisijungti prie svetimos esamos organizacijos negalima. Pakartotinis administratoriaus el. paštas atmetamas su `409`. Slaptažodžiai saugomi kaip ASP.NET `PasswordHasher` maišos.

Dviejų klientų demonstracijai pakartokite administratoriaus registraciją su kitu el. paštu:

| Duomenys | Demonstravimo pavyzdys |
| --- | --- |
| Organizacijos | Organizacija A ir Organizacija B |
| Administratoriai | Po vieną kiekvienai organizacijai |
| Aikštelės | Po vieną kiekvienai organizacijai |
| Vietos | Kiekvienoje aikštelėje pridėkite bent kelias vietas |
| Vairuotojai | Du skirtingi vairuotojai konflikto demonstracijai |

Tai pavyzdžiai, kuriuos susikuriate per sąsają, o ne iš anksto įrašytos paskyros. Rankinio SQL vykdymo nereikia.

## Įgyvendintos funkcijos ir taisyklės

- Vairuotojo bei administratoriaus registracija, prisijungimas ir atsijungimas. Nuorodos į kitą prisijungimo puslapį rodomos tik neprisijungus.
- Vairuotojas mato šiuo metu laisvas pasirinktos aikštelės vietas. Administratorius mato visas savo aikštelės vietas. Kortelės puslapiuojamos po 30; vietos numerio paieškos lauko nėra.
- Pasirinkus vietą rezervuojama nuo dabar nurodytai trukmei. API priima 1–10080 minučių; sąsajoje pasirenkama 0,25–168 valandos kas 15 minučių.
- Tikrinamas visas laikotarpis: dabar laisva vieta gali netikti ilgesnei rezervacijai, jei vėliau ji jau rezervuota.
- Persidengimas: `esama.Pradžia < nauja.Pabaiga && nauja.Pradžia < esama.Pabaiga`. Gretimi intervalai, pvz., 18:00–20:00 ir 20:00–21:00, nekonfliktuoja.
- Vairuotojas toje pačioje aikštelėje negali turėti kelių persidengiančių neatšauktų rezervacijų. Kitoje aikštelėje ar kitu metu rezervuoti galima.
- Uždaryta vieta nepriima naujų rezervacijų. Ankstesnės lieka galioti.
- Atšaukiant įrašoma nauja `Cancelled` eilutė su tuo pačiu `ReservationId`, išsaugant `Created` eilutę. Pasibaigus laikui papildomas įrašas nekuriamas; pasibaigusios rezervacijos atšaukti negalima.
- Vairuotojas mato savo rezervacijas, administratorius — savo organizacijos rezervacijas. Rezervacijos ID sąsajoje rodomas tik administratoriui.
- Vienu veiksmu pridedama 1–500 vietų. Formatą sudaro `A0` ir bent dviejų skaitmenų numeris: A001, A010, A0100. Numeracija tęsiama toje pačioje aikštelėje.
- Laikas saugomas UTC ir sąsajoje rodomas Lietuvos laiku. API taip pat leidžia rezervuoti nurodant pradžią ir pabaigą; pradžia negali būti praeityje.

## Automatiniai testai

Iš projekto aplanko:

```text
dotnet test
```

Arba Visual Studio: **Test → Test Explorer → Run All Tests**.

**Visi keturi testai veikia be papildomos konfigūracijos, MySQL ar PowerShell.** API testui automatiškai sukuriamas atskiras laikinas SQLite failas, naudojamas tikras programos schemos inicializavimas, o baigus failas pašalinamas. Naudotojo `parkspot.db` neliečiamas.

| Testas | Tikrinama |
| --- | --- |
| `Free_spot_creates_exact_requested_reservation` | Vienas rezervacijos įrašas su tikslia vieta, organizacija, pradžia, pabaiga ir būsena. |
| `Overlapping_reservation_is_rejected_without_another_row` | 3 valandų rezervacija, kai kita prasideda po 2 valandų, atmetama su `409`; papildomo įrašo nėra. |
| `Invalid_duration_is_rejected` | Nulinė trukmė atmetama su `400`; rezervacija nesukuriama. |
| `Organization_isolation_blocks_read_cancel_and_spot_changes` | Naujos organizacijos registracija ir prisijungimas; svetimų duomenų skaitymo ir keitimo atmetimas; SQLite vienalaikio rezervavimo konfliktas, atšaukimas ir išsaugoti įrašai pakartotinai inicializavus schemą. |

`TestDatabaseData` užpildo tik laikiną testų bazę: dvi organizacijas, du administratorius, du vairuotojus, po aikštelę ir šešias vietas bei būsimas rezervacijas. Įprastas programos paleidimas šių duomenų nekuria.

Rezultatus matysite terminale arba `Test Explorer`, priklausomai nuo paleidimo būdo. Sėkminga suvestinė: **4 praėję, 0 nepraėjusių, 0 praleistų**. Papildomo `PARKSPOT_TEST_MYSQL` kintamojo ar testų prisijungimo failo nebėra.

## Kodo struktūra

```text
src/ParkSpot.Web/
  Processors/              HTTP kontroleriai
  Handlers/                MediatR IRequestHandler klasės
  Services/                Verslo taisyklės, registracija ir paskyros kontekstas
    Interfaces/            Servisų sutartys
  Repositories/            SQL užklausos, vykdymas ir transakcijos
    Interfaces/            Repozitorijų sutartys
  Database/                RawSqlCommand, schemos inicializavimas, Guid konvertavimas
  Entities/
    Data/                  Lentelių duomenų klasės
    Enums/                 Rezervacijos būsenos
    Requests/              HTTP ir MediatR užklausos
    Responses/             API atsakymai
  Models/                  Paskyros kontekstas, filtrai, rezultatai ir kūrimo duomenys
  Middleware/              Bendras klaidų apdorojimas
  Exceptions/              Verslo klaidos
  wwwroot/                 HTML, CSS ir JavaScript
  Program.cs               DI, autentifikacija, DB paruošimas ir paleidimas
tests/ParkSpot.Tests/       Keturi testai
  Support/                 Laikina SQLite bazė ir testų pagalbinės klasės
Database/schema.sql        Automatiškai pritaikoma SQLite schema
docs/phase-1.md             I etapo aprašymas
```