# Analiza i projektowanie optymalnego układu klawiatury dla programistów

Ten projekt, realizowany w ramach pracy magisterskiej, skupia się na dwóch głównych zadaniach:

1. **Pobieraniu i przygotowaniu danych do analizy efektywności układów klawiatury.**
2. **Wykorzystaniu algorytmu genetycznego do optymalizacji układów klawiatury w celu poprawy efektywności pisania.**

Do implementacji algorytmu genetycznego wykorzystywana jest biblioteka GeneticSharp, a do analizy wydajności - BenchmarkDotNet.
Dodatkowo, projekt zawiera skrypt Jupyter Notebook do analizy danych i wizualizacji wyników.

## Struktura Rozwiązania

Rozwiązanie składa się z trzech projektów:

### 1. RepoDownloader

Ten projekt jest odpowiedzialny za pobieranie i filtrowanie plików z repozytoriów GitHub.

#### **Opis:**

Program pobiera repozytoria z listy zdefiniowanej w pliku `repositories.json`. Następnie, dla każdego repozytorium, wykonywane są następujące kroki:

1. **Klonowanie repozytorium:**  Repozytorium jest klonowane lokalnie za pomocą komendy `git clone` z opcjami `--filter=blob:none --no-checkout --depth 1 --sparse`, co pozwala na pobranie struktury repozytorium bez pobierania zawartości plików i tylko z ostatniego commita.
2. **Filtrowanie plików:** Z użyciem `git status --porcelain` sprawdzane są rozszerzenia plików w repozytorium. Następnie pliki są filtrowane na podstawie tablicy dozwolonych rozszerzeń: `cs`, `xaml`, `resx`, `md`, `ps1`, `csx`, `json`, `xml`, `yml`, `aspx`, `ascx`, `master`, `cshtml`, `js`, `ts`, `web.config`, `css`, `bat`, `psi`, `razor`, `sql`.
3. **Pobieranie wybranych plików:** Jeśli w repozytorium znajdują się pliki o dozwolonych rozszerzeniach, to są one pobierane do archiwum ZIP za pomocą polecenia `git archive`.
4. **Rozpakowanie i przeniesienie plików:** Archiwum ZIP jest rozpakowywane, a następnie pliki o dozwolonych rozszerzeniach są przenoszone do docelowego katalogu (`D:\\CSharpDataset`), zachowując strukturę katalogów z repozytorium. W przypadku konfliktów nazw plików, do nazwy pliku dodawany jest unikalny identyfikator.
5. **Usuwanie sklonowanego repozytorium** Po przeniesieniu plików, tymczasowe repozytorium jest usuwane.
6. **Zapisywanie postępu:** Postęp przetwarzania jest zapisywany w pliku `progress.txt`, dzięki czemu w przypadku przerwania procesu można go wznowić od ostatniego przetworzonego repozytorium.

#### **Pliki:**

-   **Program.cs:** Główny plik programu, zawierający logikę pobierania i przetwarzania repozytoriów.
-   **CommandExecutor.cs:** Klasa pomocnicza do wykonywania poleceń systemowych.
-   **Extensions.cs:** Zawiera metody rozszerzające, m.in. do formatowania ścieżek dla wiersza poleceń.
-   **RepositoryEntry.cs:** Definiuje strukturę danych dla wpisu repozytorium w pliku `repositories.json`.

#### **Uruchomienie:**

Przed uruchomieniem należy:

1. Skonfigurować ścieżki w pliku `Program.cs`:
    -   `targetDirectory`: Katalog docelowy dla pobranych plików (domyślnie `D:\\CSharpDataset`).
    -   `cloneDirectory`: Katalog tymczasowy do klonowania repozytoriów (domyślnie `C:\\Clones`).
    -   `jsonFilePath`: Ścieżka do pliku `repositories.json` z listą repozytoriów (domyślnie `C:\\dataset\\repositories.json`).
    -   `progressFilePath`: Ścieżka do pliku `progress.txt` z postępem przetwarzania (domyślnie `C:\\dataset\\progress.txt`).
2. Przygotować plik `repositories.json` z listą repozytoriów do pobrania. Każdy wpis powinien mieć format:

    ```json
    {"name": "nazwa_repozytorium", "url": "adres_api_repozytorium"}
    ```

    Przykład: `{"name": "StephenCleary/AsyncEx", "url": "https://api.github.com/repos/StephenCleary/AsyncEx"}`
3. Upewnij sie, że masz zainstalowanego klienta Git i jest on dostępny w zmiennej środowiskowej `PATH`.
4. Uruchomić projekt `RepoDownloader`.

### 2. Projekt Główny (Ewolucyjne Projektowanie Układów Klawiatury)

Ten projekt wykorzystuje pobrane dane do optymalizacji układu klawiatury za pomocą algorytmu genetycznego.

#### **Opis:**

Projekt implementuje algorytm genetyczny do optymalizacji układów klawiatury. Algorytm ten iteracyjnie ewoluuje populację układów klawiatury, dążąc do znalezienia takiego, który minimalizuje wysiłek i czas potrzebny do pisania. Wykorzystuje on zestaw danych przygotowany przez projekt `RepoDownloader` do oceny efektywności generowanych układów.

#### **Pliki:**

-   **Program.cs:** Zawiera główny punkt wejścia aplikacji. Inicjalizuje i uruchamia algorytm genetyczny.
-   **KeyboardChromosome.cs:** Reprezentuje chromosom w algorytmie genetycznym, który odpowiada układowi klawiatury.
-   **KeyboardFitness.cs:** Definiuje funkcję fitness używaną do oceny efektywności układu klawiatury.
-   **KeyboardCrossover.cs:** Implementuje operację krzyżowania, która łączy dwa chromosomy rodzicielskie, tworząc potomstwo.
-   **KeyboardMutation.cs:** Implementuje operację mutacji, która wprowadza losowe zmiany w chromosomie.
-   **FitnessScoreCalculator.cs:** Zawiera metody do obliczania różnych metryk związanych z efektywnością pisania, takich jak odległość podróży palców, siła nacisku palców, naprzemienność rąk i kierunek uderzenia.
-   **DatasetLoader.cs:** Zapewnia funkcjonalność do ładowania zestawu danych plików tekstowych do oceny układów klawiatury.
-   **Benchmark.cs:** Zawiera testy benchmarkowe do pomiaru wydajności określonych fragmentów kodu.

#### **Algorytm Genetyczny**

Rdzeniem projektu jest algorytm genetyczny, który iteracyjnie ewoluuje populację układów klawiatury, aby znaleźć optymalne rozwiązanie. Algorytm wykorzystuje następujące komponenty:

-   **Populacja:** Zbiór osobników `KeyboardChromosome` reprezentujących różne układy klawiatury.
-   **Funkcja Fitness:** Klasa `KeyboardFitness` ocenia dopasowanie każdego chromosomu na podstawie metryk, takich jak odległość podróży palca, współczynnik siły nacisku palca, naprzemienność rąk i kierunek uderzenia. Wynik dopasowania jest obliczany przy użyciu ważonej kombinacji znormalizowanych wyników z tych metryk. Do normalizacji wyników używany jest układ QWERTY.
-   **Selekcja:** Metoda `EliteSelection` wybiera najlepiej przystosowane osobniki z populacji, aby stały się rodzicami dla następnego pokolenia.
-   **Krzyżowanie:** Metoda `KeyboardCrossover` łączy dwa chromosomy rodzicielskie, aby wygenerować potomstwo z połączeniem ich cech.
-   **Mutacja:** Metoda `KeyboardMutation` wprowadza losowe zmiany w genach chromosomu (literach), aby utrzymać różnorodność w populacji.
-   **Warunek Zatrzymania:** `FitnessStagnationTermination` zatrzymuje algorytm, gdy dopasowanie najlepszego rozwiązania nie poprawia się przez określoną liczbę pokoleń.

#### **Ocena Dopasowania (Fitness)**

Klasa `KeyboardFitness` oblicza dopasowanie układu klawiatury na podstawie następujących metryk:

-   **Odległość Podróży (Travel Distance):** Całkowita odległość, jaką muszą pokonać palce, aby napisać dany tekst. Mniejsza odległość jest lepsza.
-   **Współczynnik Siły Nacisku Palca (Finger Strength Factor):** Miara wysiłku wymaganego do naciśnięcia klawiszy, biorąc pod uwagę siłę każdego palca. Mniejszy wysiłek jest lepszy.
-   **Naprzemienność Rąk (Hand Alternation):** Częstotliwość przełączania się między lewą a prawą ręką podczas pisania. Większa naprzemienność jest generalnie lepsza.
-   **Kierunek Uderzenia (Hit Direction):** Częstotliwość naciskania klawiszy w "niewłaściwym" kierunku (np. przesuwanie palca wskazującego w lewo na lewej ręce). Mniejsza liczba uderzeń w złym kierunku jest lepsza.

#### **Zestaw Danych**

Projekt wykorzystuje zestaw danych plików tekstowych, pobranych i przygotowanych przez projekt `RepoDownloader`, do oceny dopasowania układów klawiatury. Klasa `DatasetLoader` jest odpowiedzialna za ładowanie tych plików z określonego katalogu. Oczekuje się, że zestaw danych będzie miał określoną dystrybucję typów plików:

-   7,5% plików Markdown (.md)
-   7,5% plików YAML (.yml)
-   7,5% plików JSON (.json)

Moduł ładujący pobiera do 3000 plików, zachowując pożądany stosunek typów plików.

### 3. DatasetFiltering (Python)

Ten projekt, napisany w Pythonie, służy do wstępnego filtrowania i przygotowania danych potrzebnych do zasilenia projektu `RepoDownloader`.

#### **Opis:**

Skrypt przetwarza pliki JSON pobrane z GHTorrent, wykorzystując Apache Spark do szybkiego przetwarzania dużych zbiorów danych. Dla każdego pliku JSON:

1. **Wczytanie danych:** Plik JSON jest wczytywany do DataFrame'u Spark.
2. **Selekcja danych:** Tworzona jest tymczasowa tabela `github_data`, a następnie wykonywane jest zapytanie SQL, które wybiera unikalne pary `(nazwa_repozytorium, url_repozytorium)` spełniające określone warunki:
    -   Typ zdarzenia to `ForkEvent`, a język repozytorium to `C#` LUB
    -   Typ zdarzenia to `PullRequestEvent`, język repozytorium to `C#`, a liczba gwiazdek repozytorium jest większa niż 10.
3. **Łączenie wyników:** Wyniki z każdego pliku są łączone w jeden DataFrame `all_results`.
4. **Zapis wyników:** Co 10 przetworzonych plików, DataFrame `all_results` jest czyszczony z duplikatów i zapisywany do pliku JSON w katalogu `output`.

Na koniec, po przetworzeniu wszystkich plików, ostateczny DataFrame `all_results` (bez duplikatów) jest zapisywany do pliku `final_result_all.json`.

#### **Wymagania:**

-   Python 3.x
-   Apache Spark (PySpark)

#### **Uruchomienie:**

1. Zainstaluj wymagane biblioteki: `pip install pyspark`
2. Skonfiguruj zmienną `data_directory` w skrypcie, aby wskazywała na katalog z plikami JSON z GHTorrent.
3. Uruchom skrypt: `python <nazwa_skryptu>.py`

#### **Uwagi:**

-   Skrypt zakłada, że pliki JSON z GHTorrent są skompresowane gzipem i mają rozszerzenie `.json.gz`.
-   Wyniki są zapisywane w katalogu `output` w formacie JSON.

### 4. Analiza i Wizualizacja (Jupyter Notebook)

Ten projekt zawiera notebook Jupyter do analizy i wizualizacji danych.

#### **Opis:**

Notebook wykorzystuje biblioteki `pandas`, `matplotlib`, `seaborn`, `scikit-learn` i `pyLDAvis` do:

1. **Wczytania i analizy statystyk plików:**
    - Zlicza liczbę plików każdego typu (`.cs`, `.md`, `.ps1`, `.json` itp.) w zestawie danych.
    - Tworzy wykres słupkowy przedstawiający rozkład typów plików.

2. **Analizy TF-IDF:**
    - Oblicza macierz TF-IDF (Term Frequency-Inverse Document Frequency) dla słów występujących w plikach.
    - Wyświetla 10 najważniejszych terminów (słów) dla każdego dokumentu na podstawie ich wartości TF-IDF.
    - Oblicza i wyświetla średnią i odchylenie standardowe częstotliwości występowania terminów we wszystkich dokumentach.

3. **Modelowania Tematów (Topic Modeling):**
    - Wykorzystuje algorytm Latent Dirichlet Allocation (LDA) do identyfikacji tematów w zbiorze danych.
    - Wyświetla 6 zidentyfikowanych tematów wraz z 10 najważniejszymi słowami dla każdego tematu.

4. **Wizualizacji Danych:**
    - Tworzy wykresy słupkowe przedstawiające rozkład typów plików, najważniejsze terminy oraz wyniki TF-IDF.
    - Generuje interaktywną wizualizację LDA za pomocą biblioteki `pyLDAvis`.

5. **Analizy Układu Klawiatury:**
    - Oblicza i wyświetla mapy ciepła (heatmaps) dla różnych układów klawiatury (QWERTY, DVORAK, COLEMAK, COLEMAK DH) oraz dla wygenerowanych przez algorytm genetyczny układów. Mapy ciepła pokazują rozkład częstości naciskania poszczególnych klawiszy.
    - Oblicza statystyki użycia klawiatury dla różnych układów, takie jak użycie środkowego rzędu, użycie lewej ręki i użycie słabych palców.
    - Oblicza i wyświetla odległość przebytą przez palce podczas pisania dla różnych układów.
    - Oblicza i wyświetla współczynnik siły nacisku palca dla różnych układów.
    - Oblicza i wyświetla wskaźniki naprzemienności rąk i kierunku uderzenia dla różnych układów.

#### **Wymagania:**

- Python 3.x
- Biblioteki: `pandas`, `matplotlib`, `seaborn`, `scikit-learn`, `gensim`, `pyLDAvis`, `wordcloud`.

#### **Uruchomienie:**

1. Zainstaluj wymagane biblioteki (jeśli jeszcze ich nie masz):

    ```bash
    pip install pandas matplotlib seaborn scikit-learn gensim pyldavis wordcloud
    ```

2. Otwórz notebook Jupyter (`.ipynb` file) w środowisku Jupyter.
3. Uruchom wszystkie komórki w notebooku.

#### **Uwagi:**

- Upewnij się, że ścieżka do katalogu z danymi (`dataset_path`) jest poprawnie ustawiona w pierwszej komórce kodu.
- Notebook zawiera interaktywną wizualizację LDA, którą można eksplorować po uruchomieniu komórki z kodem `pyLDAvis`.

## Zależności

Projekty wchodzące w skład rozwiązania mają następujące zależności:

-   **GeneticSharp:** Biblioteka .NET do algorytmów genetycznych (używana w projekcie głównym).
-   **BenchmarkDotNet:** Biblioteka do benchmarkingu kodu .NET (używana w projekcie głównym).
-   **System.IO.Compression:** Biblioteka do obsługi archiwów ZIP (używana w projekcie `RepoDownloader`).
-   **System.Text.Json:** Biblioteka do serializacji i deserializacji JSON (używana w projekcie `RepoDownloader`).
-   **pyspark:** Biblioteka Pythona do obsługi Apache Spark (używana w projekcie `DatasetFiltering`).
-   **pandas:** Biblioteka Pythona do analizy danych (używana w projekcie `AnalysisVisualization`).
-   **matplotlib:** Biblioteka Pythona do tworzenia wykresów (używana w projekcie `AnalysisVisualization`).
-   **seaborn:** Biblioteka Pythona do tworzenia atrakcyjnych wykresów statystycznych (używana w projekcie `AnalysisVisualization`).
-   **scikit-learn:** Biblioteka Pythona do uczenia maszynowego (używana w projekcie `AnalysisVisualization`).
-   **gensim:** Biblioteka Pythona do modelowania tematów (używana w projekcie `AnalysisVisualization`).
-   **pyLDAvis:** Biblioteka Pythona do interaktywnej wizualizacji modeli LDA (używana w projekcie `AnalysisVisualization`).
-   **wordcloud:** Biblioteka Pythona do tworzenia chmur słów (używana w projekcie `AnalysisVisualization`).

## Użycie (Projekt Główny)

Aby uruchomić projekt główny:

1. **Sklonuj repozytorium:**

    ```bash
    git clone <adres-repozytorium>
    ```

2. **Przejdź do katalogu projektu:**

    ```bash
    cd <katalog-projektu>
    ```

3. **Przywróć zależności:**

    ```bash
    dotnet restore
    ```

4. **Zbuduj projekt:**

    ```bash
    dotnet build
    ```

5. **Uruchom projekt `DatasetFiltering` w celu wygenerowania pliku `repositories.json`**
6. **Uruchom projekt `RepoDownloader` w celu pobrania i przygotowania danych.**
7. **Zaktualizuj ścieżkę do zestawu danych:**
    Otwórz plik `Program.cs` w projekcie głównym i zmień ścieżkę w konstruktorze `KeyboardFitness` na swój katalog z zestawem danych:

    ```csharp
    KeyboardFitness fitness = new("E:\\CSharpDataset"); // Zastąp własną ścieżką do danych
    ```

8. **Uruchom aplikację:**

    ```bash
    dotnet run
    ```
9. **Uruchom notebook `AnalysisVisualization`**
    - Otwórz notebook Jupyter (`.ipynb` file) w środowisku Jupyter.
    - Uruchom wszystkie komórki w notebooku.

Program wypisze najlepszy wynik dopasowania dla każdego pokolenia oraz ostateczne najlepsze znalezione rozwiązanie.

## Dostosowanie

Możesz dostosować następujące aspekty algorytmu genetycznego:

-   **Rozmiar populacji:** Zmodyfikuj konstruktor `Population` w pliku `Program.cs`.
-   **Prawdopodobieństwo mutacji:** Zmień właściwość `MutationProbability` instancji `GeneticAlgorithm` w pliku `Program.cs`.
-   **Kryteria zakończenia:** Zmodyfikuj właściwość `Termination` instancji `GeneticAlgorithm`.
-   **Wagi dla metryk dopasowania:** Dostosuj wagi (`fingerTravelWeight`, `fingerStrengthWeight`, itp.) w metodzie `KeyboardFitness.Evaluate`.
-   **Początkowy układ:** Zmień zmienną `qwertyLayout` w `Program.cs`, aby rozpocząć algorytm z innym układem.
