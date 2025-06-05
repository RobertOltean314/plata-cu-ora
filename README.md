<div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
  <h1 style="margin: 0;">Plata cu ora</h1>
  <div style="display: flex; gap: 20px;">
    <img src="img/ntt-data-logo.jpg" alt="NTT Data Logo" style="width: 45%; max-width: 150px;" />
    <img src="img/ulbs-logo.png" alt="ULBS Logo" style="width: 45%; max-width: 150px; background-color: black; padding: 10px; border-radius: 8px;" />
  </div>
</div>


Aplicatie pentru gestionarea orelor lucrate de catre cadre universitare dezvoltata in cadrul proiectului TechTrek, sustinut de NTT DATA Romania.
<span style="color:orange">prezentarea echipei?</span>


# Introducere
In cadrul unei institutii de invatamant superior, viata academica reprezinta parcursul educational al unui student sau cadru didactic. Ce ar fi parcursul acesta fara o schimbare continua? In secolul XXI tehnologia se dezvolta exponential, iar schimbarea are loc la fiecare pas. 
<span style="color:red">de bagat mai multa vrajeala: adauga aplicatia de pontaj ca exemplul dezvoltarii (excel-> aplicatie). </span>
<span style="color:blue">sau spune de cum disciplina trebuie sa fie prezenta atat la studenti cat si la profesori, si de aia o aplicatie de pontaj este importanta. </span>
<span style="color:orange">sau prezinta ecuatia: ore lucrate = bani, viata - bani = naspa</span>
<span style="color:green">????totul a pornit de la nevoia de a pune in aplicare cunostiintele dobandite in urma proiectului techtrek</span>

## Context
In cadrul _Universitatii Lucian Blaga din Sibiu_, procesul de generare al fluturasului de salariu pentru un cadru didactic se realiza intr-un mod <span style="color:red">_demodat_</span> si greoi. Acest proces consista intr-un fisier Excel cu macro-uri complexe, foarte greu de optimizat si intretinut.
<span style="color:green">dezvolta: cat de naspa era procesul de completare, cat de antic si invechit pasau unii de la altii un fisier pentru generarea unei declaratii</span>
<span style="color:orange">?shoutout Canti pentru idee</span>

### Scopul aplicatiei
Aplicatia are ca scop automatizarea procesului de completare al orelor lucrate, intr-un mod placut visibil si user friendly.
<span style="color:green">dezvolta: cat de usor este acum procesul de completare al orelor, fata de cum era inainte cu excel-ul</span>
<span style="color:red">mai merge putina vrajeala ca in introducere</span>

### Public tinta
Aplicatia are ca public tinta cadre universitare de la universitatea Lucian Blaga. Cu toate acestea, are posibilitatea de se extinde prin includerea altor universitati.
<span style="color:green">dezvolta: baga vrajeala cat de importante sunt cadrele didactice pentru societate ??????? </span>
<span style="color:red">de ce facilitarea activitatilor pe langa cele didactice este triviala. de ce pentru niste loaze batrane e mai usoara aplicatia noastra decat un excel+</span>


## Tehnologii utilizate

<span style="color:red">De dezvoltat, de ce, cum, unde, cine, pentru</span>
<span style="color:green">de specificat versiuni</span>
- Angular
- Asp.net
- Firebase
- Bootstrap

## Functionalitati

Aplicatia confera un set de functionalitati esentiale ce simplifica procesul de completare al orelor lucrate de catre personalul didactic, acestea fiind: 

### Autentificare (Login)
- utilizatorii se autentifica folosind adresa de mail si parola
- odata cu succesul autentificarii, se genereaza o cheie unica de acces, prin care se asigura securitatea endpoint-urilor
<span style="color:red">Adauga tehnologiile implicate</span>
<span style="color:green">Poate specifici mai putin despre securitate si adaugi un capitol despre asta</span>
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/login_1.png" alt="Login">
</div>


### Deconectarea (Logout)
- utilizatorii se pot deconecta oricand
- cheia unica de acces este eliminata din sesiune, pastrand securitatea endpoint-urilor
- dupa deconectare, utilizatorul este redirectionat catre pagina de autentificare
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/profil_1.png" alt="Logout, profile info">
</div>


### Inregistrarea unui cont nou (Register)
- utilizatorii pot crea un cont nou prin completarea campurilor de:
    - nume
    - mail
    - parola
    - rol
- la validarea campurilor (email si confirmare parola), datele utilizatorului sunt introduse in sistem
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/register_1.png" alt="Register">
</div>


### Completarea detaliilor utilizatorului
- odata autentificat, utilizatorul isi poate completa profilul cu informatiile necesare generarii declaratiei:
    - nume si prenume
    - tip
    - director de departament
    - decan
    - universitate
    - facultate
    - departament
- aceste detalii sunt salvate in sistem
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/generare_declaratie_1.png" alt="Generare declaratie">
</div>


### Generarea declaratiei
- pe baza datelor introduse aplicatia genereaza o declaratie personalizata cu datele utilizatorului, numarul total de ore lucrate si calculul sumei
- formatul este similar cu cel utilizat in Excel-ul folosit anterior, <span style="color: red">si produce un seniment de homesick cadrelor didactice</span>
<div style="display: flex; justify-content: center;">
  <img src="./img/old_excel/old_declaratie.png" alt="declaratie 2" style="width: 80%">
</div>

- declaratia este in format pdf si poate fi descarcata/ vizualizata
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/declaratie_generata_1.png" alt="declaratie 1" style="width: 90%">
</div>
<div style="display: flex; justify-content: center;">
  <img src="./img/demo/declaratie_generata_2.png" alt="declaratie 2" style="width: 90%">
</div>



### Mentiuni
- aplicatia tine cont de paritatea saptamaniilor
- aplicatia tine cont de zilele libere, folosind un api cu sarbatorile legale




## Securitate ? 
## Proces de dezvoltare ?
## Testare ?x
## Hosting ? 
## Probleme intampinate ?x
## Posibilitati de dezvoltare ?x
## Concluzii finale ? Feedback-ul cadrelor didactice ?x

### schema baza de date, framework-uri, tool-uri


