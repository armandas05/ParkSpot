# ParkSpot

# Parkavimo vietų rezervavimo sistema

## Kursinio darbo I dalis: projektavimo dokumentas

## 1. Problema ir idėja

**Sistema vienu sakiniu:**
Sistema skirta privačių ir riboto patekimo parkavimo aikštelių naudotojams bei jų valdytojams ir leis peržiūrėti aikštelės vietų užimtumą, pasirinkti bei rezervuoti parkavimo vietą norimam laikotarpiui.

**Problema ir dabartinis procesas:**
Privačiose parkavimo aikštelėse, pavyzdžiui, prie bendrabučių, daugiabučių ar įmonių, vairuotojai ne visada žino, ar atvykę ras laisvą parkavimo vietą. Dažnai laisvos vietos ieškoma tik atvykus į aikštelę ir fiziškai ją apvažiuojant. Papildoma problema yra netinkamas arba nesąžiningas parkavimas, kai vairuotojas užima kitam asmeniui priklausančią ar jo rezervuotą vietą. Tokiu atveju rezervaciją turintis vairuotojas atvykęs nebegali pasinaudoti jam skirta vieta. Dėl to vairuotojai sugaišta laiko ieškodami vietos, kyla konfliktų tarp aikštelės naudotojų, o aikštelės valdytojui sudėtingiau kontroliuoti jos užimtumą.

**Nauda:**
Sistema leis vairuotojui prieš atvykstant peržiūrėti parkavimo aikštelę, matyti vietų prieinamumą pasirinktu laikotarpiu ir iš anksto rezervuoti tinkamą vietą. Tai turėtų sumažinti laiką, praleidžiamą ieškant laisvos vietos, ir padėti efektyviau išnaudoti aikštelės vietas. Aikštelės valdytojui sistema suteiks galimybę valdyti parkavimo vietas ir stebėti jų rezervacijas.

**Naudotojai:**
Numatomi du pagrindiniai sistemos naudotojų tipai:

* **Vairuotojas** – galės peržiūrėti parkavimo aikšteles ir jų vietas, pasirinkti norimą laikotarpį bei rezervuoti laisvą parkavimo vietą.
* **Parkavimo aikštelės valdytojas** – galės valdyti aikštelės parkavimo vietas, jų prieinamumą ir peržiūrėti rezervacijas.

**Prielaidos:**
Daroma prielaida, kad kiekviena sistemoje registruota parkavimo aikštelė turi iš anksto apibrėžtas ir sunumeruotas parkavimo vietas. Taip pat laikoma, kad aikštelės valdytojas pateikia teisingą informaciją apie vietas ir jų prieinamumą. Prototipe fizinis automobilio buvimas konkrečioje vietoje nebus automatiškai nustatomas naudojant kameras ar parkavimo jutiklius, todėl sistema negalės automatiškai nustatyti, ar vairuotojas automobilį pastatė būtent savo rezervuotoje vietoje. Ateityje ši problema galėtų būti sprendžiama integruojant papildomas parkavimo kontrolės priemones. Fizinis šlagbaumas prototipe taip pat gali būti imituojamas programinėje sistemoje.

---

## 2. Apimtis

| Funkcija                           | Ką naudotojas galės atlikti                                                                                              | Pagrindinis modulis ar pagalbinė funkcija |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------- |
| Parkavimo vietos rezervavimas      | Pasirinkti aikštelę, parkavimo laikotarpį ir rezervuoti tinkamą laisvą vietą                                             | **Pagrindinis modulis**                   |
| Aikštelės ir vietų peržiūra        | Peržiūrėti aikštelės planą, parkavimo vietas ir jų prieinamumą pasirinktu laikotarpiu                                    | Pagalbinė funkcija                        |
| Aikštelės vietų valdymas           | Aikštelės valdytojui pridėti parkavimo vietas, nustatyti jų tipą ir prieinamumą                                          | Pagalbinė funkcija                        |
| Įvažiavimo informacijos pateikimas | Nuskenavus prie aikštelės esantį QR kodą atidaryti konkrečios aikštelės informaciją ir parodyti galimas parkavimo vietas | Pagalbinė funkcija                        |

**Į kursinio darbo apimtį neįeina:**
Pirmoje sistemos versijoje neplanuojama įgyvendinti realaus bankinių kortelių mokėjimų apdorojimo, automobilių numerių atpažinimo kameromis, fizinių parkavimo vietų užimtumo jutiklių ir tiesioginės integracijos su realiu šlagbaumu. Mobiliąją programėlę planuojama svarstyti kaip tolimesnę sistemos plėtros kryptį, kai bus sukurtas veikiantis internetinės sistemos prototipas.

---

## 3. Pagrindinis modulis

**Pavadinimas ir atsakomybė:**
Parkavimo vietos rezervavimo modulis. Jo atsakomybė – pagal naudotojo pasirinktą aikštelę ir laikotarpį nustatyti, ar pasirinkta parkavimo vieta gali būti rezervuota, išvengti persidengiančių rezervacijų ir, esant galimybei, sukurti rezervaciją.

**Logika, kurią reikės projektuoti ir testuoti:**
Pagrindinę modulio logiką sudarys parkavimo vietos prieinamumo tikrinimas ir rezervacijų konfliktų nustatymas. Sistema turės patikrinti, ar pasirinkta vieta egzistuoja ir yra prieinama rezervacijoms, ar pasirinktas laikotarpis yra tinkamas ir ar tuo laikotarpiu vieta nėra rezervuota kito naudotojo. Jei pasirinktos vietos rezervuoti negalima, sistema turės atmesti rezervaciją ir informuoti naudotoją apie priežastį.

**Įvestis:**
Aikštelės identifikatorius, parkavimo vietos identifikatorius, rezervacijos pradžios ir pabaigos laikas bei rezervaciją atliekantis naudotojas.

Pavyzdys:

* Aikštelė: A
* Parkavimo vieta: A06
* Rezervacijos pradžia: 2026-10-10 18:00
* Rezervacijos pabaiga: 2026-10-10 21:00

**Išvestis:**
Sėkmingai sukurta rezervacija arba informacija, kodėl rezervacijos sukurti nepavyko.

Sėkmingo rezultato pavyzdys:

> Parkavimo vieta A06 rezervuota 2026-10-10 nuo 18:00 iki 21:00.

**Veikimo eiga:**

1. Naudotojas pasirenka parkavimo aikštelę ir norimą rezervacijos laikotarpį.
2. Sistema pateikia tuo laikotarpiu prieinamas parkavimo vietas.
3. Naudotojas pasirenka norimą vietą.
4. Sistema dar kartą patikrina pasirinktos vietos prieinamumą.
5. Patikrinama, ar rezervacijos laikotarpis nepersidengia su jau egzistuojančia tos vietos rezervacija.
6. Jei konfliktų nėra, rezervacija išsaugoma.
7. Naudotojui pateikiamas rezervacijos patvirtinimas. Jei rezervacija negalima, pateikiama jos atmetimo priežastis.

### Taisyklės arba sprendimo žingsniai

1. Rezervacijos pabaigos laikas turi būti vėlesnis už jos pradžios laiką.
2. Vienai parkavimo vietai negali egzistuoti dvi laike persidengiančios aktyvios rezervacijos.
3. Rezervuoti galima tik egzistuojančią ir rezervacijoms prieinamą parkavimo vietą.

### Scenarijai būsimiems testams

| Scenarijus                        | Pradinės sąlygos ir konkreti įvestis                                                       | Veiksmas                                   | Tikslus laukiamas rezultatas                                                                                             |
| --------------------------------- | ------------------------------------------------------------------------------------------ | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| Įprastas atvejis                  | A06 yra laisva 2026-10-10 nuo 18:00 iki 21:00                                              | Naudotojas rezervuoja A06 nurodytam laikui | Sukuriama rezervacija A06 vietai nuo 18:00 iki 21:00 ir naudotojui pateikiamas patvirtinimas                             |
| Ribinis atvejis arba konfliktas   | A06 jau rezervuota nuo 18:00 iki 20:00. Naudotojas bando rezervuoti ją nuo 19:30 iki 21:00 | Pateikiamas rezervacijos prašymas          | Rezervacija nesukuriama, nes laikotarpiai persidengia. Naudotojas informuojamas, kad vieta pasirinktu laiku neprieinama  |
| Klaida arba neįmanomas rezultatas | Naudotojas pasirenka rezervacijos pradžią 21:00, o pabaigą 18:00                           | Pateikiamas rezervacijos prašymas          | Rezervacija nesukuriama ir pateikiamas pranešimas, kad rezervacijos pabaigos laikas turi būti vėlesnis už pradžios laiką |

**Jei modulis naudoja AI:**
Netaikoma. Pagrindinis rezervavimo modulis naudos iš anksto apibrėžtas verslo taisykles.

---

## 4. Kokybės atributas

**Pasirinktas atributas:**
Patikimumas.

**Kodėl svarbus šiai sistemai:**
Parkavimo rezervavimo sistemoje svarbu užtikrinti, kad ta pati parkavimo vieta nebūtų rezervuota keliems naudotojams tuo pačiu metu. Tokia klaida sukeltų konfliktą realioje parkavimo aikštelėje ir sumažintų naudotojų pasitikėjimą sistema.

**Tikrinimo scenarijus ir sąlygos:**
Bus imituojama situacija, kai du naudotojai beveik tuo pačiu metu bando rezervuoti tą pačią parkavimo vietą tam pačiam laikotarpiui.

**Sėkmės kriterijus:**
Iš dviejų konfliktuojančių rezervacijos užklausų tik viena turi būti sėkmingai išsaugota. Duomenų bazėje negali atsirasti dvi aktyvios tos pačios parkavimo vietos rezervacijos su persidengiančiais laikotarpiais.

**Numatytas projektavimo sprendimas:**
Rezervacijos kūrimas bus vykdomas duomenų bazės transakcijoje. Prieš tikrinant pasirinktos parkavimo vietos prieinamumą, atitinkamas parkavimo vietos įrašas bus užrakinamas transakcijos laikotarpiui. Kol pirmoji transakcija tikrina esamas rezervacijas ir kuria naują rezervaciją, kita transakcija, bandanti rezervuoti tą pačią vietą, turės laukti, kol pirmoji bus užbaigta. Gavusi prieigą antroji transakcija iš naujo patikrins vietos prieinamumą ir aptiks jau sukurtą rezervaciją, todėl konfliktuojanti rezervacija bus atmesta. Užraktas bus laikomas tik rezervacijos patikrinimo ir sukūrimo metu, kad kuo mažiau būtų ribojamos kitos sistemos operacijos.

**Kaip patikrinsiu vėlesniame etape:**
Bus sukurtas integracinis testas, kuriame dvi lygiagrečiai vykdomos užklausos bandys rezervuoti tą pačią parkavimo vietą persidengiančiam laikotarpiui. Bus patikrinta, kad pirmoji transakcija rezervavimo metu užrakina vietą, antroji negali atlikti konfliktuojančios rezervacijos iki pirmosios transakcijos pabaigos, o po pakartotinio prieinamumo patikrinimo viena iš rezervacijų yra atmetama. Galutinis kriterijus – duomenų bazėje lieka tik viena iš dviejų konfliktuojančių rezervacijų.

**Sprendimo kaina arba ribojimas:**
Papildomi prieinamumo patikrinimai ir transakcijų naudojimas apsunkina rezervavimo logiką ir gali šiek tiek padidinti užklausos vykdymo laiką, tačiau padeda užtikrinti duomenų vientisumą.

---

## 5. Pradinė sistemos struktūra

### Paprasta schema

```text
┌──────────────────────┐
│     Naudotojas       │
│ Naršyklė / telefonas │
└──────────┬───────────┘
           │
           │   HTTP / JSON
           ▼
┌──────────────────────┐
│      Frontend        │
│    Web aplikacija    │
└──────────┬───────────┘
           │
           │   REST API
           ▼
┌────────────────────────────┐
│       ASP.NET Core API     │
│                            │
│  ┌──────────────────────┐  │
│  │ Rezervavimo modulis  │  │
│  └──────────────────────┘  │
│                            │
│  ┌──────────────────────┐  │
│  │ Aikštelių valdymas   │  │
│  └──────────────────────┘  │
└──────────────┬─────────────┘
               │
               │ Entity Framework Core
               ▼
┌──────────────────────────────┐
│        Duomenų bazė          │
│ Aikštelės, vietos,           │
│ naudotojai, rezervacijos     │
└──────────────────────────────┘
```

| Sistemos dalis       | Atsakomybė                                                                                                                                                                                                           |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Web aplikacija       | Pateikti naudotojo sąsają aikštelių ir vietų peržiūrai bei rezervacijų atlikimui. Sąsaja turės būti pritaikyta ir didesnėms aikštelėms, kuriose visas parkavimo vietas vienu metu atvaizduoti gali būti nepraktiška. |
| ASP.NET Core Web API | Priimti naudotojo užklausas, vykdyti verslo logiką ir perduoti duomenis tarp naudotojo sąsajos bei duomenų bazės                                                                                                     |
| Rezervavimo modulis  | Tikrinti vietos prieinamumą, aptikti rezervacijų konfliktus ir kurti rezervacijas                                                                                                                                    |
| Duomenų bazė         | Saugoti naudotojų, aikštelių, parkavimo vietų ir rezervacijų duomenis                                                                                                                                                |

**Planuojamos technologijos ir pasirinkimo priežastys:**
Serverio daliai planuojama naudoti C# ir ASP.NET Core Web API, nes ši platforma tinkama REST tipo interneto paslaugoms ir leidžia aiškiai atskirti sistemos sluoksnius bei verslo logiką. Duomenų bazės operacijoms planuojama naudoti Entity Framework Core, o duomenų saugojimui – MySQL reliacinę duomenų bazę. Naudotojo sąsajai planuojama naudoti interneto technologijas, galimai React, kad sistema būtų pasiekiama tiek kompiuterio, tiek telefono naršyklėje.

Pirmiausia planuojama sukurti veikiančią internetinės sistemos prototipo versiją. Turint veikiantį pagrindinės sistemos prototipą, ateityje planuojama apsvarstyti ir atskiros mobiliosios programėlės kūrimą. Kadangi pagrindinė sistemos verslo logika bus pasiekiama per Web API, mobili programa galėtų naudoti tą pačią serverio dalį ir duomenų bazę kaip ir internetinė aplikacija.

---

## 6. AI panaudojimas

### AI rengiant šį dokumentą

Rengiant pradinį projektavimo dokumentą buvo naudojamas generatyvinio AI įrankis.

| Priemonė ir užduotis                                             | Ką panaudojau                                                                                                             | Ką atmečiau arba perrašiau ir kodėl                                                                                                                                                                                                                                   | Kaip patikrinau                                                                                                                           |
| ---------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- |
| ChatGPT – sistemos struktūra ir pradinės architektūros aptarimas | Panaudotos idėjos rezervavimo modulio riboms, rezervacijų konfliktų scenarijams ir pradinei sistemos struktūrai apibrėžti | Atsisakyta dalies papildomų funkcijų, tokių kaip realūs mokėjimai, automobilių numerių atpažinimas ir IoT parkavimo jutikliai, nes jos per daug išplėstų pradinio prototipo apimtį. AI pasiūlytas tekstas buvo peržiūrėtas ir pritaikytas pasirinktos sistemos idėjai | Patikrinau, ar pasiūlymai atitinka užduoties reikalavimus, ar numatytą pagrindinio modulio logiką galima įgyvendinti ir atskirai testuoti |

### Planuojamas AI naudojimas kuriant sistemą

**Kur ir kam naudosiu AI:**
AI planuojama naudoti kaip pagalbinę priemonę kuriant programinį kodą, analizuojant galimas klaidas, generuojant ir atliekant automatinius testus.

**Kaip tikrinsiu pasiūlymus ir sugeneruotą kodą:**
AI sugeneruotas kodas nebus naudojamas jo neperžiūrėjus. Bus tikrinama jo veikimo logika, suderinamumas su likusia sistema ir vykdomi vienetų bei integraciniai testai. Sugeneruoti sprendimai prireikus bus keičiami arba perrašomi.

**Ar AI bus sistemos funkcionalumo dalis:**
Ne. Šiuo metu AI nėra planuojamas kaip parkavimo rezervavimo sistemos funkcionalumo dalis. Pagrindinio modulio sprendimai bus priimami pagal apibrėžtas ir testuojamas verslo taisykles.

---

## 7. Tolesnių darbų planas

| Darbas                                | Apčiuopiamas rezultatas                                                                   | Planuojama darbų seka |
| ------------------------------------- | ----------------------------------------------------------------------------------------- | --------------------- |
| Suprojektuoti duomenų modelį          | Apibrėžtos pagrindinės esybės ir jų ryšiai: naudotojai, aikštelės, vietos ir rezervacijos | 1                     |
| Sukurti ASP.NET Core Web API pagrindą | Veikiantis API projektas ir ryšys su duomenų baze                                         | 2                     |
| Įgyvendinti rezervavimo modulį        | Veikiantis vietos prieinamumo tikrinimas, konfliktų aptikimas ir rezervacijos sukūrimas   | 3                     |
| Sukurti bazinę naudotojo sąsają       | Naudotojas gali pasirinkti aikštelę, laiką, peržiūrėti vietas ir pateikti rezervaciją     | 4                     |
| Sukurti pagrindinio modulio testus    | Automatiniai testai pagrindinėms rezervavimo taisyklėms ir konfliktų scenarijams          | 5                     |

**Būsimo prototipo veikimo scenarijus:**
Prototipo demonstracijos metu naudotojas pasirinks parkavimo aikštelę ir nurodys, kad nori parkuotis 2026-10-10 nuo 18:00 iki 21:00. Sistema pateiks tuo laikotarpiu prieinamas parkavimo vietas. Naudotojas pasirinks, pavyzdžiui, A06 vietą ir pateiks rezervaciją. Sistema patikrins jos prieinamumą ir sukurs rezervaciją. Pakartotinai bandant rezervuoti A06 persidengiančiam laikotarpiui, sistema turės atmesti rezervaciją ir informuoti apie konfliktą.

| Rizika arba neaiškumas                                                                                                                               | Kaip patikrinsiu arba sumažinsiu                                                                                                                                                                                                                                                                                    |
| ---------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Vienu metu pateiktos rezervacijos gali sukurti tos pačios vietos rezervavimo konfliktą                                                               | Sukursiu integracinius testus vienalaikėms rezervacijoms ir rezervacijos išsaugojimui naudosiu transakciją bei pakartotinį prieinamumo patikrinimą                                                                                                                                                                  |
| Interaktyvaus aikštelės plano realizavimas gali užimti per daug laiko                                                                                | Pirmoje prototipo versijoje naudosiu supaprastintą aikštelės vietų atvaizdavimą. Sudėtingesnį vizualų planą įgyvendinsiu tik tuo atveju, jei liks pakankamai laiko                                                                                                                                                  |
| Pasirinktos technologijos ar sistemos struktūra įgyvendinimo metu gali pasirodyti per sudėtinga                                                      | Pirmiausia įgyvendinsiu minimalų veikimo scenarijų nuo rezervacijos užklausos iki jos išsaugojimo.                                                                                                                                                                                                                  |
| Didelėse parkavimo aikštelėse visų parkavimo vietų atvaizdavimas viename plane gali būti nepatogus naudotojui, ypač naudojantis mobiliuoju įrenginiu | Kuriant prototipą bus išbandyti skirtingi aikštelės atvaizdavimo būdai. Didelės aikštelės galėtų būti skirstomos į zonas ar sektorius, o naudotojui pirmiausia būtų rodoma pasirinkta aikštelės dalis ir joje esančios vietos. Taip pat bus svarstomas plano priartinimas, nutolinimas ir laisvų vietų filtravimas. |


## Šaltiniai, jei naudojote

Papildomi išoriniai šaltiniai rengiant pradinę sistemos idėją nebuvo naudojami. AI naudojimas aprašytas 6 skyriuje.
