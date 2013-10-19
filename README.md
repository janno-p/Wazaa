# Vorgurakendused 2 (2013) #

## Praktikum 1: Wazaa ##

*Praktikumi kirjeldus on võetud aadressilt
[http://lambda.ee/wiki/Vorgurakendused_2_prax_1_2013](http://lambda.ee/wiki/Vorgurakendused_2_prax_1_2013),
mille materjalid on kasutatavad [GNU Free Documentation License 1.2](http://www.gnu.org/copyleft/fdl.html)
tingimustel.*

[Wazaa](http://www.youtube.com/watch?v=GCZ4rhE6pww) on [Gnutella](http://en.wikipedia.org/wiki/Gnutella)
ja [Kazaa](http://en.wikipedia.org/wiki/Kazaa) väike vend.

Ülesandeks on ehitada P2P rakendus nimega [wazaa](http://www.youtube.com/watch?v=GCZ4rhE6pww)
failide otsimiseks võõrastest masinatest, mis teeb laviinitaolise otsingu mööda wazaa
klient/servereid, isetehtud P2P mehhanismi abil.

Kasutajaliidese hea kvaliteet ei ole praksis oluline.

Huvi korral võid vaadata ka veidi sarnaseid vanu ülesandeid
[2012 aasta arhiivis](http://lambda.ee/wiki/V%C3%B5rgurakendused_2012_prax_1) ning
[2010 ja 2011 aasta arhiivis](http://lambda.ee/wiki/Vorgurakendused_2_prax_1).


### Sisukord ###

  1. [Üldkirjeldus](#uldkirjeldus)
  1. [Miks just selline päringu edasisaatmisviis?](#edastusviisist)
  1. [Tehnilised nõuded](#nouded)
  1. [Protokoll](#protokoll)
     1. [Otsipäringu saatmine ja edasisaatmine](#saatmine)
     1. [Leitud failist teadaandmine algsele pärijale](#teadaandmine)
     1. [Leitud faili küsimine](#kysimine)
  1. [Praksi kaks osa](#osad)
  1. [Soovitusi ja ideid](#soovitused)
  1. [Firewall ja NAT](#firewall)
  1. [Punktiarvestusest](#punktiarvestus)


### <a name="uldkirjeldus" />Üldkirjeldus ###

Wazaa võimaldab:

  * kasutajal otsida võõrastest masinatest faile failinimes sisalduva stringi (näiteks "Film1")
    järgi.

    * Seejuures otsitakse ainult wazaa kataloogis olevaid faile, mitte ei otsita kogu masinat läbi.
      wazaa kataloogis ei ole alamkatalooge.
    * Leitud failid kuvatakse loetelus, kus on võõra masina IP, port ja terviklik failinimi.

  * kasutajal tõmmata endale faili kuvatud loetelust, teades masina IP-d, porti ja failinime.

Failide leidmiseks kasutab wazaa järgmist viisi:

  * Saadab tema oma masinas oleva faili machines.txt masinatele failiotsingu päringu. machines.txt
    sees on masinate ip ja pordid json listina kujul

        [["11.22.33.44","2345"],...,["111.222.333.444","23456"]]

  * Samamoodi saadab failiotsingu-päringu wazaa startlehel
    [http://dijkstra.cs.ttu.ee/~tammet/wazaa.txt](http://dijkstra.cs.ttu.ee/~tammet/wazaa.txt)
    olevatele masinatele (startlehel on plaintekstina eelmises punktis näidatud formaadis json-fail)
  * Kui mingi masin leiab minu otsitud faili, siis ta saadab mulle teate, et mis IP ja pordiga masin
    ja mis täisnimega fail on.
  * Iga võõras masin, mis algselt minu käest tulnud päringu saab, saadab ta omakorda kõigile talle
    teadaolevatele edasi, vähendades seejuures nn ttl (time to live) counti, et päringud lõpmatult
    rändama ei jääks.

Ehk, masinad saadavad minu antud päringut rekursiivselt ise edasi.

Pane tähele, et sinu wazaa ei pea üldse salvestama võõraste wazaade ip-sid ja porte! Samas ei ole ka
kuidagi keelatud (debugimiseks või niisama huvi pärast vms) salvestada teiste wazaade ip-sid, mida
saab teada näiteks sulle tulnud päringutest, sulle vastajate ip-dest ja miks mitte ka portscannides
:)


### <a name="edastusviisist" />Miks just selline päringu edasisaatmisviis? ###

Põhimõtteliselt saab päringut massidesse P2P meetoditega laiali saata kahel viisil:

  * **Iteratiivne meetod** (antud praksis seda ei kasuta): algne masin küsib tuttavatelt neile
    tuttavate masinate faile, ja suurendab seeläbi endale teadaolevate masinate hulka. Igalt uuelt
    teadasaadud masinalt küsib ta jälle selle masina tuttavate faile jne. Teised masinad algset
    päringut ise kuhugi edasi ei saada. Meetodi pluss on see, et algne masin saab lihtsalt tagada,
    et päringut ei saadetaks kaks korda samale masinale. Meetodi miinus on see, et kõik masinad
    võrgus saadavad lõpuks terve neile tuttavate masinate faili algsele masinale: see tähendab väga
    suurt hulka väga suurte failide saatmisi, mis hakkavad jõhkralt ummistama võrku, ja algse masina
    väike kanal tõenäoliselt ummistub varsti. Saadud failide ühisosad on väga suured. Halvemal juhul
    saab algne masin N identset sõnumit, kus igaühes on N ip-d, kus N on kõigi masinate arv, kus
    jookseb wazaa.

  * **Rekursiivne meetod** (antud praksis nõutud): algne masin saadab oma päringu tuttavatele
    masinatele, kes selle omakorda enda tuttavatele edasi saadavad jne. Igüks, kes leiab enda
    failide hulgast vajaliku faili, saadab info algsele masinale (vastasel korral ei saada ta
    algsele masinale midagi). Algne masin ei suurenda endale teadaolevate masinate hulka. Meetodi
    pluss on see, et algsele masinale saadetakse tagasi infot ainult siis, kui vajalik info leitakse
    (väga väike protsent masinaid) ja see info on lühike. Koormus võrgule langeb tugevasti ja algse
    masina kanal tõenäoliselt ei ummistu. Meetodi miinus on asjaolu, et üks ja sama masin võib saada
    algse päringu mitu korda eri suundadest, mis jällegi kasvatab võrgu koormust. Seda viimast
    probleemi saab mitmel moel leevendada (kuigi mitte täielikult kaotada), eeskätt päringu ttl
    (time to live) parameetrit kasutades, päringu ahela masinate lisamises päringu parameetrite
    hulka jne.

Gnutella kasutab rekursiivset meetodit, nii ka meie wazaa. Lihtsakoeline rekursiivne meetod ummistab
suure klientide arvu korral võrgu ja sele asemel kasutavad suured P2P süsteemid kas:

  * struktureeritud nn superpeeride süsteemi a la Kazaa ja esialgne Skype, kus rekursiivne päring
    toimub ainult superpeeride vahel, ja neid on vähe, või
  * väga kavalat nn distributed hash tables (DHT) mehhanismi, a la freenet ja (osaliselt)
    bittorrent. DHT on üks mitmest bittorrenti poolt kasutatud meetodist, kuigi bittorrenti
    põhifookus on hoopis faili paralleelse allatõmbamise optimeerimisel mitme masina koostöös, mitte
    masinate/failide leidmisel. Või:
  * kasutavad masinate leidmiseks eeskätt tsentraalseid servereid, a la esialgne Napster või
    kaasaegne Skype


### <a name="nouded" />Tehnilised nõuded ###

Programmeerimiskeele ja muu tehnoloogia valik on vaba, kuid rakendus peab vastama neile
tingimustele:

  * Kogu programm peab töötama kas AK klassi arvutis või sinu oma läptopis, mis AK klassis kaasas.
    Mingit kaugel asuvat serverit põhirakenduse jaoks kasutada ei tohi. Arusaadavalt võib aga panna
    wazaad lisaks käima ka suvalistesse välistesse masinatesse, et neilt faile küsida.
  * Http protokolli jms realisatsioon peab olema teie oma programmi osa, mingit "suurt" http
    serverit a la apache kasutada ei või. Kõikvõimalike võrguteekide (versus eraldiseisvad
    veebiserverid) kasutamine on ok, mikroserverite koodi integreerimine oma programmi on ok.


### <a name="protokoll" />Protokoll ###

#### <a name="saatmine" />Otsipäringu saatmine ja edasisaatmine ####

Konkreetselt masinalt 11.22.33.44 (kus wazaa jookseb pordis 2345) sobivate failide pärimine

    http://11.22.33.44:2345/searchfile?name=filename&sendip=55.66.77.88&sendport=6788&ttl=5&id=wqeqwe23&noask=11.22.33.44_111.222.333.444 ...

kus parameetritel on järgmine sisu:

  * name: otsitav string (peab olema failinimes)
  * sendip: esialgse otsija ip
  * sendport: esialgse otsija port
  * ttl: kui mitu otsingu-hopi veel teha (iga edasiküsimine vähendab ühe võrra)
  * id: optsionaalne päringu identifikaator
  * noask: optsionaalne list masinatest, kellelt pole mõtet küsida (eraldajaks alakriips)

Päringu vastuseks on õnnestunud päringu korral number 0, ebaõnnestunud päringu korral muu
(vea)number. Vastuse mõte on eeskätt debugimise lihtsustamine ja vastust peaks pärija üldiselt
ignoreerima. Wazaa peaks töötama edukalt ka juhul, kui ta saatmise peale ühenduse lihtsalt kinni
paneb ja vastust kuulama ei hakka.

Wazaale tulnud päringu edasisaatmine käib täpselt sama protokolliga, kuid edasi saadetakse veidi
muudetud päring:

  * ttl vähendatakse ühe võrra
  * optionaalselt: lisatakse oma masina ip noask parameetrile otsa

Seejuures **ei saadeta päringut edasi**, kui sissetulnud ttl oli 1 võ i vähem (st ttl=0 päringuid
enam kuhugi ei saadeta)

Seda päringut saab ka brauserist katsetada, kuigi brauser ei saa vastuseks midagi huvitavat peale
debugimis-vastuse 0 või mingi veakoodi.

#### <a name="teadaandmine" />Leitud failist teadaandmine algsele pärijale ####

post call urlile

    http://11.22.33.44:2345/foundfile

kus IP ja port on algse pärija omad ning post päringu sisu (peale headeris olevat tühja rida) on
json formaadis list failide täisnimedega a la

    { "id": "wqeqwe23",
      "files":
      [
        {"ip":"11.22.33.66", "port":"5678", "name":"minufail1.txt"},
        ...
        {"ip":"11.22.33.68", "port":"5678", "name":"xxfail1yy.txt"}
      ]
    }

kus elementidel on järgmine sisu:

  * id: esialgse päringu id (kui oli algselt antud)
  * name: täispikk leitud faili nimi
  * ip: masina ip, kus antud fail asub
  * port: masina wazaa port, kus antud fail asub

Postituse saaja vastab eduka kättesaamise korral arvu 0, arusaamatuste korral mõne muu (vea) numbri.
Jällegi, vastuse mõte on eeskätt debugimise lihtsustamine ja vastust peaks pärija üldiselt
ignoreerima. Wazaa peaks töötama edukalt ka juhul, kui ta saatmise peale ühenduse lihtsalt kinni
paneb ja vastust kuulama ei hakka.

#### <a name="kysimine" />Leitud faili küsimine ####

Konkreetselt masinalt 11.22.33.44 (kus wazaa jookseb pordis 2345) konkreetse faili küsimine:

    http://11.22.33.44:2345/getfile?fullname=fullfilename

Viimasele, getfile päringule vastatakse faili saatmisega üle http protokolli (st headeris on
kindlasti ka content-length ja mime type): seda päringut peaks katseks saama edukalt ka brauserist
teha ja brauser peaks faili alla laadima.


### <a name="osad" />Praksi kaks osa ###

Väga soovitav on teha praks kahes osas ja esitada kaks korda ülevaatamiseks:

  * Teha http protokolli järgida suutev esmane rakendus.
  * Lisada sinna kogu vajalik funktsionaalsus.

Http protokolli järgimise võimet kontrollitakse praksi eduka läbimise tingimusena.


### <a name="soovitused" />Soovitusi ja ideid ###

Arendamiseks ja testimiseks pead saama jooksutada oma masinas mitut erinevat koopiat omaenda
wazaast, igaüks oma pordil, igaühel oma konfifail tuttavate masinatega ja oma kataloog jagatavate
failidega.

Hea mõte on teha wazaa käsurealt käivitavaks nii, et ta võtab kohe käsurealt pordinumbri
parameetriks ning loeb siis oma konfifaili ja jagatavaid faile vastavast pordinumbrist tuletatud
failipathist.

Wazaa peab päris kindlasti sisaldama või kasutama http serverit, muidu ei ole temaga võimalik
väljast ühendust võtta. NB! See ei tähenda mingit nö suurt veebiserverit a la apache, vaid pigem ise
kirjutatud väikest minimaalset serverit, mis pealegi ei pea veebilehti serveerima ja cgi-sid
käivitama, vaid ainult teatud käskudele/parameetritele reageerima.

Server tuleb realiseerida ise, kasutades võrgust leitavaid näiteid mikro-http-serveri jaoks
(tüüpiliselt paar lehekülge koodi). Selliseid näitekoode võib täiesti otse kasutada.

C jaoks on sobiv mikroserver näiteks [tiny](http://www.lambda.ee/images/8/89/Itsissejuhatus_tiny.zip),
java jaoks on [see server](http://www.java2s.com/Code/Java/Tiny-Application/HttpServer.htm) väga hea
väike näide, aga selgitava tekstita, ning selgitavate tekstidega servereid leiad mh
[siit](http://onjava.com/pub/a/onjava/2003/04/23/java_webserver.html) ja
[siit](http://fragments.turtlemeat.com/javawebserver.php). Loe java socketitest põhjalikumalt
[siit](http://download.oracle.com/javase/tutorial/networking/sockets/). Pythoni serverinäite
[saad siit](http://muharem.wordpress.com/2007/05/29/roll-your-own-server-in-50-lines-of-code/). Http
kohta võib lugeda [näiteks siit](http://www.jmarshall.com/easy/http/).

Kasutajaliides võib olla tehtud aknaga, aga see ei ole kohustuslik: võib teha ka puhtalt käsurea
rakenduse.

Rakendus on mõistlik teha nii, et annad käivitamisel ette pordi, millel ta räägib, ning kui seda
porti ei ole antud, siis kasutad vaikimisi porti 1215 (1214 on Kazaa port).

Häid selgeid näiteid ja õpetusi socketite ja serveri tegemise kohta leiad veel siit:

  * [socketite kasutamise õpetus ja source C-s, javas ja pythonis](http://www.prasannatech.net/2008/07/socket-programming-tutorial.html)
  * [lihtne veebiserver C-s (eksootilise sanosi all)](http://www.jbox.dk/sanos/webserver.htm)
  * [lihtne veebiserver/klient C-s (ibm-i tutorial ja source)](http://www.ibm.com/developerworks/systems/library/es-nweb/index.html)
  * [tiny webserver C-s](http://www.lambda.ee/images/8/89/Itsissejuhatus_tiny.zip)
  * [java socketid: klassikaline tutorial](http://download.oracle.com/javase/tutorial/networking/sockets/)
  * [Kaarel Alliku kursuse näite-jutukas (java)](http://www.tud.ttu.ee/material/kallik/JOOP/Jutukas/)
  * [java http teegi kasutamine, mis kasutab socketeid sisemiselt](http://www.rgagnon.com/javadetails/java-have-a-simple-http-server.html)
  * [ruby](http://tomayko.com/writings/unicorn-is-unix)
  * [python](http://jacobian.org/writing/python-is-unix/)
  * [pythoni üks serversocketi-teek](http://docs.python.org/library/socketserver.html)
  * [Väga põhjalik socketite/võrguprogrammeerimise õpetus](http://beej.us/guide/bgnet/)


### <a name="firewall" />Firewall ja NAT ###

Firewallist läbi murdmise ja kinniste portidega ühendamise probleemiga ei pea selles praksis
tegelema. Samas tähendab see, et ei ole kuigi lihtne teise masinaga tegelikult ühendust saada (ühes
masinas debugimise võimaldamine ongi seepärast kriitiline).

Kes wazaa käima saab, võib mõelda välja ja realiseerida probleemi lahendamiseks sobiva vaheserveri:
selle eest saab ekstrapunkte.

Võrgust infot otsides leiad kahte liiki teemasid, meie jaoks oluline on siit listist teine:

  * Tunneldamise artiklid, loe näiteks [http tunnel](http://en.wikipedia.org/wiki/HTTP_tunnel)
    artiklit wikipediast ja [seda artiklit](http://sebsauvage.net/punching/) firewallist
    läbipääsemisest.

  * UDP/TCP hole punching ja NAT traversal: arusaadava sissejuhatusena loe
    [Skype jms süsteemide meetoditest](http://www.h-online.com/security/features/How-Skype-Co-get-round-firewalls-747197.html),
    sellele [väike täiendus](http://www.portugal-a-programar.org/forum/index.php?topic=7852.0).
    Vaata ka [wikipedia utiliitide artiklit ja viiteid](http://en.wikipedia.org/wiki/Session_Traversal_Utilities_for_NAT).
    Detailsemalt/lisaks [siit](http://www.brynosaurus.com/pub/net/p2pnat/) ja
    [siit](http://nutss.gforge.cis.cornell.edu/pub/imc05-tcpnat/) ja
    [siit](http://www.jdrosen.net/midcom_turn.html) ja
    [siit](http://www.ietf.org/rfc/rfc3489.txt), keerulise vahendi leiab
    [siit](http://natblaster.sourceforge.net/).

NB! Seda kõike ei ole väga lihtne teha ja sinu wazaa (ja ka selle inimese wazaa, kellega suhtled)
peab ka oskama seda vaheserverit kasutada. Ära hakka seda ehitama, kui nö lihtne wazaa veel ei
tööta!

Ekstra-lisapunktide saamise variant: realiseeri ja pane kõigile kasutatavaks firewalliprobleemi
lahendamiseks sobiv vaheserver. Kirjuta kasutamise juhend ja näited, mida teised saaks soovi korral
oma wazaa jaoks tarvitada (st panna nad sinu vaheserverit kasutama, nii et seda saaks ka
arvutiklassist kasutada). Esimesed kaks gruppi, kes sellega hakkama saavad, saavad lisaks pooled
praksipunktid!


### <a name="punktiarvestus" />Punktiarvestusest ###

Kui kõik eelnimetatu töötab vastu mõne teise grupi wazaad ja koodi suudetakse seletada, saab
täispunktid.

Kui teiste wazaa-programmidega ei suuda suhelda, aga iseenda koopiatega siiski, siis päris
täispunkte ei saa.
