# Ewolucyjne Projektowanie Uk�ad�w Klawiatury (Praca Magisterska)

Ten projekt, realizowany w ramach pracy magisterskiej, skupia si� na dw�ch g��wnych zadaniach:

1. **Pobieraniu i przygotowaniu danych do analizy efektywno�ci uk�ad�w klawiatury.**
2. **Wykorzystaniu algorytmu genetycznego do optymalizacji uk�ad�w klawiatury w celu poprawy efektywno�ci pisania.**

Do implementacji algorytmu genetycznego wykorzystywana jest biblioteka GeneticSharp, a do analizy wydajno�ci - BenchmarkDotNet.
Dodatkowo, projekt zawiera skrypt Jupyter Notebook do analizy danych i wizualizacji wynik�w.

## Struktura Rozwi�zania

Rozwi�zanie sk�ada si� z czterech projekt�w:

### 1. RepoDownloader

Ten projekt jest odpowiedzialny za pobieranie i filtrowanie plik�w z repozytori�w GitHub.

#### **Opis:**

Program pobiera repozytoria z listy zdefiniowanej w pliku `repositories.json`. Nast�pnie, dla ka�dego repozytorium, wykonywane s� nast�puj�ce kroki:

1. **Klonowanie repozytorium:** Repozytorium jest klonowane lokalnie za pomoc� komendy `git clone` z opcjami `--filter=blob:none --no-checkout --depth 1 --sparse`, co pozwala na pobranie struktury repozytorium bez pobierania zawarto�ci plik�w i tylko z ostatniego commita.
2. **Filtrowanie plik�w:** Z u�yciem `git status --porcelain` sprawdzane s� rozszerzenia plik�w w repozytorium. Nast�pnie pliki s� filtrowane na podstawie tablicy dozwolonych rozszerze�: `cs`, `xaml`, `resx`, `md`, `ps1`, `csx`, `json`, `xml`, `yml`, `aspx`, `ascx`, `master`, `cshtml`, `js`, `ts`, `web.config`, `css`, `bat`, `psi`, `razor`, `sql`.
3. **Pobieranie wybranych plik�w:** Je�li w repozytorium znajduj� si� pliki o dozwolonych rozszerzeniach, to s� one pobierane do archiwum ZIP za pomoc� polecenia `git archive`.
4. **Rozpakowanie i przeniesienie plik�w:** Archiwum ZIP jest rozpakowywane, a nast�pnie pliki o dozwolonych rozszerzeniach s� przenoszone do docelowego katalogu (`D:\\CSharpDataset`), zachowuj�c struktur� katalog�w z repozytorium. W przypadku konflikt�w nazw plik�w, do nazwy pliku dodawany jest unikalny identyfikator.
5. **Usuwanie sklonowanego repozytorium** Po przeniesieniu plik�w, tymczasowe repozytorium jest usuwane.
6. **Zapisywanie post�pu:** Post�p przetwarzania jest zapisywany w pliku `progress.txt`, dzi�ki czemu w przypadku przerwania procesu mo�na go wznowi� od ostatniego przetworzonego repozytorium.

#### **Uruchomienie:**

Przed uruchomieniem nale�y:

1. Skonfigurowa� �cie�ki w pliku `Program.cs`:
   - `targetDirectory`: Katalog docelowy dla pobranych plik�w (domy�lnie `D:\\CSharpDataset`).
   - `cloneDirectory`: Katalog tymczasowy do klonowania repozytori�w (domy�lnie `C:\\Clones`).
   - `jsonFilePath`: �cie�ka do pliku `repositories.json` z list� repozytori�w (domy�lnie `C:\\dataset\\repositories.json`).
   - `progressFilePath`: �cie�ka do pliku `progress.txt` z post�pem przetwarzania (domy�lnie `C:\\dataset\\progress.txt`).
2. Przygotowa� plik `repositories.json` z list� repozytori�w do pobrania. Ka�dy wpis powinien mie� format:

   ```json
   { "name": "nazwa_repozytorium", "url": "adres_api_repozytorium" }
   ```

   Przyk�ad: `{"name": "StephenCleary/AsyncEx", "url": "https://api.github.com/repos/StephenCleary/AsyncEx"}`

3. Upewnij sie, �e masz zainstalowanego klienta Git i jest on dost�pny w zmiennej �rodowiskowej `PATH`.
4. Uruchomi� projekt `RepoDownloader`.

### 2. Projekt G��wny (master_thesis)

Ten projekt wykorzystuje pobrane dane do optymalizacji uk�adu klawiatury za pomoc� algorytmu genetycznego.

#### **Opis:**

Projekt implementuje algorytm genetyczny do optymalizacji uk�ad�w klawiatury. Algorytm ten iteracyjnie ewoluuje populacj� uk�ad�w klawiatury, d���c do znalezienia takiego, kt�ry minimalizuje wysi�ek i czas potrzebny do pisania. Wykorzystuje on zestaw danych przygotowany przez projekt `RepoDownloader` do oceny efektywno�ci generowanych uk�ad�w.

#### **Algorytm Genetyczny**

Rdzeniem projektu jest algorytm genetyczny, kt�ry iteracyjnie ewoluuje populacj� uk�ad�w klawiatury, aby znale�� optymalne rozwi�zanie. Algorytm wykorzystuje nast�puj�ce komponenty:

- **Populacja:** Zbi�r osobnik�w `KeyboardChromosome` reprezentuj�cych r�ne uk�ady klawiatury.
- **Funkcja Fitness:** Klasa `KeyboardFitness` ocenia dopasowanie ka�dego chromosomu na podstawie metryk, takich jak odleg�o�� podr�y palca, wsp�czynnik si�y nacisku palca, naprzemienno�� r�k i kierunek uderzenia. Wynik dopasowania jest obliczany przy u�yciu wa�onej kombinacji znormalizowanych wynik�w z tych metryk. Do normalizacji wynik�w u�ywany jest uk�ad QWERTY.
- **Selekcja:** Metoda `EliteSelection` wybiera najlepiej przystosowane osobniki z populacji, aby sta�y si� rodzicami dla nast�pnego pokolenia.
- **Krzy�owanie:** Metoda `KeyboardCrossover` ��czy dwa chromosomy rodzicielskie, aby wygenerowa� potomstwo z po��czeniem ich cech.
- **Mutacja:** Metoda `KeyboardMutation` wprowadza losowe zmiany w genach chromosomu (literach), aby utrzyma� r�norodno�� w populacji.
- **Warunek Zatrzymania:** `FitnessStagnationTermination` zatrzymuje algorytm, gdy dopasowanie najlepszego rozwi�zania nie poprawia si� przez okre�lon� liczb� pokole�.

#### **Ocena Dopasowania (Fitness)**

Klasa `KeyboardFitness` oblicza dopasowanie uk�adu klawiatury na podstawie nast�puj�cych metryk:

- **Odleg�o�� Podr�y (Travel Distance):** Ca�kowita odleg�o��, jak� musz� pokona� palce, aby napisa� dany tekst. Mniejsza odleg�o�� jest lepsza.
- **Wsp�czynnik Si�y Nacisku Palca (Finger Strength Factor):** Miara wysi�ku wymaganego do naci�ni�cia klawiszy, bior�c pod uwag� si�� ka�dego palca. Mniejszy wysi�ek jest lepszy.
- **Naprzemienno�� R�k (Hand Alternation):** Cz�stotliwo�� prze��czania si� mi�dzy lew� a praw� r�k� podczas pisania. Wi�ksza naprzemienno�� jest generalnie lepsza.
- **Kierunek Uderzenia (Hit Direction):** Cz�stotliwo�� naciskania klawiszy w "niew�a�ciwym" kierunku (np. przesuwanie palca wskazuj�cego w lewo na lewej r�ce). Mniejsza liczba uderze� w z�ym kierunku jest lepsza.

#### **Zestaw Danych**

Projekt wykorzystuje zestaw danych plik�w tekstowych, pobranych i przygotowanych przez projekt `RepoDownloader`, do oceny dopasowania uk�ad�w klawiatury. Klasa `DatasetLoader` jest odpowiedzialna za �adowanie tych plik�w z okre�lonego katalogu. Oczekuje si�, �e zestaw danych b�dzie mia� okre�lon� dystrybucj� typ�w plik�w:

- 7,5% plik�w Markdown (.md)
- 7,5% plik�w YAML (.yml)
- 7,5% plik�w JSON (.json)

Modu� �aduj�cy pobiera do 3000 plik�w, zachowuj�c po��dany stosunek typ�w plik�w.

### 3. Projekt DatasetFiltering (Python)

Ten projekt, napisany w Pythonie, s�u�y do wst�pnego filtrowania i przygotowania danych potrzebnych do zasilenia projektu `RepoDownloader`.

#### **Opis:**

Skrypt przetwarza pliki JSON pobrane z GithubArchive, wykorzystuj�c Apache Spark do szybkiego przetwarzania du�ych zbior�w danych. Dla ka�dego pliku JSON:

1. **Wczytanie danych:** Plik JSON jest wczytywany do DataFrame'u Spark.
2. **Selekcja danych:** Tworzona jest tymczasowa tabela `github_data`, a nast�pnie wykonywane jest zapytanie SQL, kt�re wybiera unikalne pary `(nazwa_repozytorium, url_repozytorium)` spe�niaj�ce okre�lone warunki:
   - Typ zdarzenia to `ForkEvent`, a j�zyk repozytorium to `C#` LUB
   - Typ zdarzenia to `PullRequestEvent`, j�zyk repozytorium to `C#`, a liczba gwiazdek repozytorium jest wi�ksza ni� 10.
3. **��czenie wynik�w:** Wyniki z ka�dego pliku s� ��czone w jeden DataFrame `all_results`.
4. **Zapis wynik�w:** Co 10 przetworzonych plik�w, DataFrame `all_results` jest czyszczony z duplikat�w i zapisywany do pliku JSON w katalogu `output`.

Na koniec, po przetworzeniu wszystkich plik�w, ostateczny DataFrame `all_results` (bez duplikat�w) jest zapisywany do pliku `final_result_all.json`.

#### **Wymagania:**

- Python 3.x
- Apache Spark (PySpark)

#### **Uruchomienie:**

1. Zainstaluj wymagane biblioteki: `pip install pyspark`
2. Skonfiguruj zmienn� `data_directory` w skrypcie, aby wskazywa�a na katalog z plikami JSON z GHTorrent.
3. Uruchom skrypt: `python <nazwa_skryptu>.py`

#### **Uwagi:**

- Skrypt zak�ada, �e pliki JSON z GithubArchive s� skompresowane gzipem i maj� rozszerzenie `.json.gz`.
- Wyniki s� zapisywane w katalogu `output` w formacie JSON.

### 4. Projekt Analiza i Wizualizacja (Jupyter Notebook)

Ten projekt zawiera notebook Jupyter do analizy i wizualizacji uk�ad�w klawiatur.

#### **Opis:**

Notebook wykorzystuje biblioteki `pandas`, `matplotlib`, `seaborn`, `scikit-learn` i `pyLDAvis` do:

1. **Wczytania i analizy statystyk plik�w:**

   - Zlicza liczb� plik�w ka�dego typu (`.cs`, `.md`, `.ps1`, `.json` itp.) w zestawie danych.
   - Tworzy wykres s�upkowy przedstawiaj�cy rozk�ad typ�w plik�w.

2. **Analizy TF-IDF:**

   - Oblicza macierz TF-IDF (Term Frequency-Inverse Document Frequency) dla s��w wyst�puj�cych w plikach.
   - Wy�wietla 10 najwa�niejszych termin�w (s��w) dla ka�dego dokumentu na podstawie ich warto�ci TF-IDF.
   - Oblicza i wy�wietla �redni� i odchylenie standardowe cz�stotliwo�ci wyst�powania termin�w we wszystkich dokumentach.

3. **Modelowania Temat�w (Topic Modeling):**

   - Wykorzystuje algorytm Latent Dirichlet Allocation (LDA) do identyfikacji temat�w w zbiorze danych.
   - Wy�wietla 6 zidentyfikowanych temat�w wraz z 10 najwa�niejszymi s�owami dla ka�dego tematu.

4. **Wizualizacji Danych:**

   - Tworzy wykresy s�upkowe przedstawiaj�ce rozk�ad typ�w plik�w, najwa�niejsze terminy oraz wyniki TF-IDF.
   - Generuje interaktywn� wizualizacj� LDA za pomoc� biblioteki `pyLDAvis`.

5. **Analizy Uk�adu Klawiatury:**
   - Oblicza i wy�wietla mapy ciep�a (heatmaps) dla r�nych uk�ad�w klawiatury (QWERTY, DVORAK, COLEMAK, COLEMAK DH) oraz dla wygenerowanych przez algorytm genetyczny uk�ad�w. Mapy ciep�a pokazuj� rozk�ad cz�sto�ci naciskania poszczeg�lnych klawiszy.
   - Oblicza statystyki u�ycia klawiatury dla r�nych uk�ad�w, takie jak u�ycie �rodkowego rz�du, u�ycie lewej r�ki i u�ycie s�abych palc�w.
   - Oblicza i wy�wietla odleg�o�� przebyt� przez palce podczas pisania dla r�nych uk�ad�w.
   - Oblicza i wy�wietla wsp�czynnik si�y nacisku palca dla r�nych uk�ad�w.
   - Oblicza i wy�wietla wska�niki naprzemienno�ci r�k i kierunku uderzenia dla r�nych uk�ad�w.

#### **Wymagania:**

- Python 3.x
- Biblioteki: `pandas`, `matplotlib`, `seaborn`, `scikit-learn`, `gensim`, `pyLDAvis`, `wordcloud`.

#### **Uruchomienie:**

1. Zainstaluj wymagane biblioteki (je�li jeszcze ich nie masz):

   ```bash
   pip install pandas matplotlib seaborn scikit-learn gensim pyldavis wordcloud
   ```

2. Otw�rz notebook Jupyter (`.ipynb` file) w �rodowisku Jupyter.
3. Uruchom wszystkie kom�rki w notebooku.

#### **Uwagi:**

- Upewnij si�, �e �cie�ka do katalogu z danymi (`dataset_path`) jest poprawnie ustawiona w pierwszej kom�rce kodu.
- Notebook zawiera interaktywn� wizualizacj� LDA, kt�r� mo�na eksplorowa� po uruchomieniu kom�rki z kodem `pyLDAvis`.

## Zale�no�ci

Projekty wchodz�ce w sk�ad ca�ego rozwi�zania maj� nast�puj�ce zale�no�ci:

- **GeneticSharp:** Biblioteka .NET do algorytm�w genetycznych (u�ywana w projekcie g��wnym).
- **BenchmarkDotNet:** Biblioteka do benchmarkingu kodu .NET (u�ywana w projekcie g��wnym).
- **System.Text.Json:** Biblioteka do serializacji i deserializacji JSON (u�ywana w projekcie `RepoDownloader`).
- **pyspark:** Biblioteka Pythona do obs�ugi Apache Spark (u�ywana w projekcie `DatasetFiltering`).
- **pandas:** Biblioteka Pythona do analizy danych (u�ywana w projekcie `AnalysisVisualization`).
- **matplotlib:** Biblioteka Pythona do tworzenia wykres�w (u�ywana w projekcie `AnalysisVisualization`).
- **seaborn:** Biblioteka Pythona do tworzenia atrakcyjnych wykres�w statystycznych (u�ywana w projekcie `AnalysisVisualization`).
- **scikit-learn:** Biblioteka Pythona do uczenia maszynowego (u�ywana w projekcie `AnalysisVisualization`).
- **gensim:** Biblioteka Pythona do modelowania temat�w (u�ywana w projekcie `AnalysisVisualization`).
- **pyLDAvis:** Biblioteka Pythona do interaktywnej wizualizacji modeli LDA (u�ywana w projekcie `AnalysisVisualization`).
- **wordcloud:** Biblioteka Pythona do tworzenia chmur s��w (u�ywana w projekcie `AnalysisVisualization`).

## U�ycie (Projekt G��wny)

Aby uruchomi� projekt g��wny:

1. **Sklonuj repozytorium:**

   ```bash
   git clone <adres-repozytorium>
   ```

2. **Przejd� do katalogu projektu:**

   ```bash
   cd <katalog-projektu>
   ```

3. **Przywr�� zale�no�ci:**

   ```bash
   dotnet restore
   ```

4. **Zbuduj projekt:**

   ```bash
   dotnet build
   ```

5. **Uruchom projekt `DatasetFiltering` w celu wygenerowania pliku `repositories.json`**
6. **Uruchom projekt `RepoDownloader` w celu pobrania i przygotowania danych.**
7. **Zaktualizuj �cie�k� do zestawu danych:**
   Otw�rz plik `Program.cs` w projekcie g��wnym i zmie� �cie�k� w konstruktorze `KeyboardFitness` na sw�j katalog z zestawem danych:

   ```csharp
   KeyboardFitness fitness = new("E:\\CSharpDataset"); // Zast�p w�asn� �cie�k� do danych
   ```

8. **Uruchom aplikacj�:**

   ```bash
   dotnet run
   ```

9. **Uruchom notebook `AnalysisVisualization`**
   - Otw�rz notebook Jupyter (`.ipynb` file) w �rodowisku Jupyter.
   - Uruchom wszystkie kom�rki w notebooku.

Program wypisze najlepszy wynik dopasowania dla ka�dego pokolenia oraz ostateczne najlepsze znalezione rozwi�zanie.

## Dostosowanie

Mo�esz dostosowa� nast�puj�ce aspekty algorytmu genetycznego:

- **Rozmiar populacji:** Zmodyfikuj konstruktor `Population` w pliku `Program.cs`.
- **Prawdopodobie�stwo mutacji:** Zmie� w�a�ciwo�� `MutationProbability` instancji `GeneticAlgorithm` w pliku `Program.cs`.
- **Kryteria zako�czenia:** Zmodyfikuj w�a�ciwo�� `Termination` instancji `GeneticAlgorithm`.
- **Wagi dla metryk dopasowania:** Dostosuj wagi (`fingerTravelWeight`, `fingerStrengthWeight`, itp.) w metodzie `KeyboardFitness.Evaluate`.
- **Pocz�tkowy uk�ad:** Zmie� zmienn� `qwertyLayout` w `Program.cs`, aby rozpocz�� algorytm z innym uk�adem.
