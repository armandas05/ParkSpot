# ParkSpot

Programų sistemų projektavimo kursinio darbo II etapas - veikiantis parkavimo vietų rezervavimo prototipas. I etapo aprašymas: [docs/phase-1.md](docs/phase-1.md).

Vairuotojas gali rezervuoti vietą bet kurios sistemoje esančios organizacijos aikštelėje. Organizacijos administratorius valdo tik savo organizacijos aikšteles, vietas ir rezervacijas. Pagrindinis modulis - rezervavimas su verslo taisyklėmis, konfliktų apdorojimu ir duomenų išsaugojimu MySQL.

## Įgyvendintos funkcijos

- Vairuotojo ir administratoriaus registracija, prisijungimas bei atsijungimas. Administratoriaus registracija sukuria naują organizaciją. Nuoroda į kitą prisijungimo puslapį rodoma tik neprisijungus.
- Aikštelės pasirinkimas ir numeruotų vietų kortelės, po 30 vietų puslapyje. Vairuotojas mato šiuo metu laisvas vietas, administratorius - visas savo pasirinktos aikštelės vietas.
- Pasirinkus vietą, rezervuojama nuo dabartinio laiko nurodytai trukmei. Tikrinamas visas laikotarpis, įskaitant būsimas rezervacijas.
- Savo rezervacijų peržiūra ir atšaukimas. Rezervacijos identifikatorius sąsajoje rodomas tik administratoriui.
- Administratoriaus aikštelių kūrimas, vietų pridėjimas pagal kiekį ir vietos uždarymas naujoms rezervacijoms.
- Organizacijų duomenų atskyrimas sistemos logikoje ir SQL užklausose.
- Keturi automatiniai testai: trys rezervavimo modulio ir vienas organizacijų izoliavimo testas.

## Įrankiai ir technologijos

| Įrankis | Versija / paskirtis |
| --- | --- |
| .NET SDK | 10.0; projektas tikrintas su 10.0.402 |
| ASP.NET Core | .NET 10, HTTP kontroleriai ir autentifikacija slapukais |
| MySQL Community Server | 8.4 LTS; vietinis skriptas naudoja 8.4.13 |
| Dapper | 2.1.89 |
| MySql.Data | 26.7.0 |
| MediatR | 12.5.0 |
| xUnit | Testai; tikslios paketų versijos yra testų `.csproj` |
| PowerShell | Windows PowerShell 5.1 arba PowerShell 7 vietiniams skriptams |

Sąsaja parašyta HTML, CSS ir JavaScript. React, Node.js, Docker ir išorinių paslaugų API raktų paleidimui nereikia. Windows MySQL vykdymui gali reikėti Microsoft Visual C++ x64 Redistributable.

## Skriptų paleidimas per Windows failų naršyklę

PowerShell skriptus galima paleisti ir neįvedant komandos:

1. Atidarykite projekto `scripts` aplanką.
2. Paspauskite dešinį pelės mygtuką ant reikiamo `.ps1` failo.
3. Pasirinkite **Run with PowerShell** („Vykdyti naudojant PowerShell“). Windows 11 šis punktas gali būti skiltyje **Show more options** („Rodyti daugiau parinkčių“).

Pirmam paruošimui paleiskite `Setup-Local.ps1` ir palaukite, kol jis baigs darbą. Tada programai paleisti naudokite `Start-Local.ps1`. Kol naudojate programą, jos PowerShell langą laikykite atidarytą. MySQL sustabdyti skirtas `Stop-Database.ps1`.

Taip galima paleisti ir `Test-Local.ps1`, tačiau skriptui baigus PowerShell langas gali užsidaryti. **Testų rezultatams ar klaidoms peržiūrėti skriptą paleiskite jau atidarytame terminale**, naudodami toliau pateiktas komandas. Jei „Run with PowerShell“ parinkties nėra arba vykdymą blokuoja PowerShell politika, naudokite dokumentuotą komandą su `-ExecutionPolicy Bypass`; organizacijos taikomi apribojimai vis tiek gali neleisti vykdyti skriptų.

## Pirmas paruošimas Windows aplinkoje

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Setup-Local.ps1
```

Skriptas:

1. Atsisiunčia MySQL į `.local` ir patikrina archyvo kontrolinę sumą.
2. Inicializuoja vietinį MySQL serverį, pasiekiamą per `127.0.0.1:3307`; sisteminė tarnyba nekuriama.
3. Sukuria `parkspot` bazę ir lenteles pagal [Database/schema.sql](Database/schema.sql).
4. Sukuria programai skirtą DB naudotoją su atsitiktiniu slaptažodžiu ir `SELECT`, `INSERT`, `UPDATE`, `DELETE` teisėmis.
5. Įrašo programos prisijungimą į `src/ParkSpot.Web/appsettings.Local.json`, vietinius testų nustatymus - į `.local/test-settings.json` ir atkuria .NET paketus.

Duomenys saugomi `.local/mysql-data`. Šio aplanko netrinkite norėdami tik paleisti programą iš naujo.

**Paruošimas sukuria tuščias lenteles, bet ne organizacijas, administratorių paskyras ar aikšteles.** Organizaciją ir administratoriaus paskyrą susikurkite per administratoriaus puslapio registracijos formą.

## Paleidimas ir sustabdymas

Po pradinio paruošimo prototipas paleidžiamas viena komanda:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Start-Local.ps1
```

Skriptas paleidžia MySQL, jei ji neveikia, ir ASP.NET programą.

- Vairuotojo puslapis: [http://localhost:5091](http://localhost:5091).

Programa pati MySQL serverio nepaleidžia, schemos nekeičia ir pradinių duomenų nepildo. Pakartotinai paleidus naudojami išsaugoti duomenys.

Programą terminale sustabdykite `Ctrl+C`. MySQL lieka veikti. Jai sustabdyti:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Stop-Database.ps1
```

### Paleidimas per Visual Studio

Atidarykite `ParkSpot.slnx` su .NET 10 palaikančia Visual Studio versija, pasirinkite `ParkSpot.Web` kaip paleidžiamą projektą. MySQL turi būti paruošta ir paleista atskirai.

Po `Setup-Local.ps1` MySQL lieka veikti. Vėliau jai paleisti galima naudoti `Start-Local.ps1`, tuomet sustabdyti tik jo paleistą ASP.NET programą su `Ctrl+C` ir paleisti projektą per Visual Studio. Vienu metu nepaleiskite dviejų programos kopijų tuo pačiu prievadu. Jei kompiliuojant užrakintas `ParkSpot.Web.exe`, sustabdykite ankstesnį programos paleidimą.

## Automatiniai testai

Greitas paleidimas:

```powershell
dotnet test
```

Jei nenustatytas `PARKSPOT_TEST_MYSQL`, vykdomi trys verslo logikos testai, o API izoliavimo testas pažymimas `Skipped`. Taip gali nutikti ir Visual Studio lange `Test → Test Explorer`, paspaudus `Run All Tests`.

Visiems keturiems testams, kai vietinė MySQL jau veikia:

```powershell
powershell -ExecutionPolicy Bypass -File ./scripts/Test-Local.ps1
```

Skriptas perskaito `.local/test-settings.json`, nustato `PARKSPOT_TEST_MYSQL` savo procesui ir paleidžia `dotnet test`. Pats MySQL serverio nepaleidžia.

| Testas | Tikrinamas rezultatas |
| --- | --- |
| `Free_spot_creates_exact_requested_reservation` | Sukuriamas vienas įrašas su tikslia vieta, organizacija, pradžia, pabaiga ir `Created` būsena. |
| `Overlapping_reservation_is_rejected_without_another_row` | 3 valandų rezervacija, kai kita prasideda po 2 valandų, atmetama su `409`; papildomo įrašo nėra. |
| `Invalid_duration_is_rejected` | Nulinė trukmė atmetama su `400`; įrašas nesukuriamas. |
| `Organization_isolation_blocks_read_cancel_and_spot_changes` | Per registracijos API sukuriama nauja organizacija ir administratorius, patikrinamas pakartotinio el. pašto atmetimas bei prisijungimas, sukuriama aikštelė ir vieta. Svetimos organizacijos rezervacijos skaitymas ir atšaukimas, vietos keitimas bei vietų pridėjimas atmetami su `404`; tikrinami savo organizacijos duomenys ir nepakeista svetima vieta bei rezervacija. |

Izoliavimo testas vykdo HTTP užklausas per ASP.NET testų serverį ir naudoja tikrą MySQL. Sukuriama atsitiktinė `parkspot_test_<GUID>` bazė, įvykdoma projekto schema, paruošiami testų duomenys, o baigus pašalinama tik ši testų bazė. Įprasta `parkspot` bazė neliečiama.

Naudojant kitą MySQL serverį, nustatykite `PARKSPOT_TEST_MYSQL` prisijungimo eilutę testų DB naudotojui, turinčiam teises kurti, naudoti ir šalinti testų bazes, ir vykdykite `dotnet test`.

Rezultatai rodomi tame pačiame terminale. Sėkmingo visų testų paleidimo suvestinėje turi būti **4 praėję, 0 nepraėjusių ir 0 praleistų**. Nepraėjus testui pateikiamas jo pavadinimas ir klaidos detalės. Terminalo paleidimo rezultatai automatiškai nepatenka į Visual Studio `Test Explorer`; `Test-Local.ps1` atskiro rezultatų failo nekuria.

## Pagrindinės verslo taisyklės

- Rezervacijos pabaiga turi būti vėlesnė už pradžią, pradžia negali būti praeityje.
- Rezervuojant nuo dabar API priima 1–10080 minučių trukmę; sąsajoje pasirenkama 0,25–168 valandų trukmė kas 15 minučių.
- Vietos užimtumas tikrinamas visam pasirinktam laikotarpiui. Dabar laisva vieta gali būti netinkama ilgai rezervacijai, jei vėliau jau yra kitas užsakymas.
- Persidengimas: `esama.Pradžia < nauja.Pabaiga && nauja.Pradžia < esama.Pabaiga`. Gretimi intervalai, pvz., 18:00–20:00 ir 20:00–21:00, nekonfliktuoja.
- Vairuotojas toje pačioje aikštelėje negali turėti kelių persidengiančių neatšauktų rezervacijų. Kitoje aikštelėje arba kitu laiku rezervuoti galima.
- Uždaryta vieta nepriima naujų rezervacijų. Ankstesnės rezervacijos lieka galioti.
- Atšaukimas įterpia naują `Cancelled` įrašą su tuo pačiu `ReservationId`; pradinis `Created` įrašas lieka. Pasibaigus laikui papildomas įrašas nekuriamas, pasibaigusios rezervacijos atšaukti negalima.
- Vienu veiksmu pridedama 1–500 vietų. Numeriai sudaromi iš `A0` ir bent dviejų skaitmenų eilės numerio: A001, A010, A0100. Numeracija tęsiama pasirinktoje aikštelėje.
- Laikas bazėje saugomas UTC, sąsajoje rodomas Lietuvos laiku.

Rezervavimas naudoja `READ COMMITTED` transakciją ir `SELECT ... FOR UPDATE` vietos bei vairuotojo eilutėms. Atšaukimas užrakina vietos eilutę. Tikrinimas ir įrašymas vykdomi toje pačioje transakcijoje.

## Organizacijų duomenų atskyrimas

Organizacija yra sistemos klientas. `Managers`, `ParkingLots`, `ParkingSpots` ir `Reservations` turi `OrganizationId`. Vairuotojai (`Users`) bendri visai platformai, todėl gali rezervuoti skirtingų organizacijų aikštelėse.

Prisijungusio administratoriaus organizacija ir naudotojo ID paimami iš autentifikuotos paskyros per `CurrentActor`, o ne iš naudotojo pateikto JSON. Repozitorijos riboja skaitymą ir keitimą pagal šį kontekstą. Vairuotojas mato tik savo rezervacijas. Svetimi identifikatoriai neleidžia administratoriui perskaityti ar pakeisti kitos organizacijos duomenų; tai tikrina API testas.

## Kodo struktūra

```text
src/ParkSpot.Web/
  Processors/              API kontroleriai
  Handlers/                MediatR IRequestHandler klasės
  Services/                Verslo logika, autentifikacija ir paskyros kontekstas
    Interfaces/            Servisų sutartys
  Repositories/            SQL užklausos, jų vykdymas ir transakcijos
    Interfaces/            Repozitorijų sutartys
  Database/                RawSqlCommand ir SQL parametrai
  Entities/
    Data/                  Duomenų lentelių klasės
    Enums/                 Rezervacijos būsenos
    Requests/              HTTP ir MediatR užklausos
    Responses/             API atsakymų klasės
  Models/                  Paskyros kontekstas ir užklausų rezultatai
  Middleware/              Bendras klaidų apdorojimas
  Exceptions/              Verslo klaidos
  wwwroot/                 HTML, CSS ir JavaScript
  Program.cs               DI, autentifikacija, autorizacija ir programos paleidimas
tests/ParkSpot.Tests/       Trys modulio testai ir vienas API izoliavimo testas
  Support/                 Testų DB paruošimas, laikrodis ir repozitorijų pakaitalai
Database/schema.sql        Lentelių, indeksų ir ryšių schema
scripts/                   Paruošimo, paleidimo, testų ir DB sustabdymo skriptai
docs/phase-1.md             I etapo aprašymas
```
