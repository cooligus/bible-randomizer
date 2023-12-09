using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;
using System.Collections.ObjectModel;
using static System.Reflection.Metadata.BlobBuilder;

namespace BibleRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        // Kotwica do naszego API 
        const string baseURI = "https://www.biblia.info.pl";
        const string infoPostfix = "/api/info/bt";
        const string basePostfix = "/api/biblia/bt";

        private ObservableCollection<string> booksList;
        private ObservableCollection<int> chaptersList;
        private ObservableCollection<int> allVersesList;
        private ObservableCollection<int> selectableVersesList;

        bool error = false;

        public MainWindow()
        {
            InitializeComponent();
            booksList = new ObservableCollection<string>();
            chaptersList = new ObservableCollection<int>();
            allVersesList = new ObservableCollection<int>();
            selectableVersesList = new ObservableCollection<int>();

            cmbBook.ItemsSource = booksList;
            cmbChapter.ItemsSource = chaptersList;
            cmbStart.ItemsSource = allVersesList;
            cmbEnd.ItemsSource = selectableVersesList;

        }

        private async Task<string> GetJSON(string url)
        {
            // Tworzymy więź emocjonalną z naszym API 
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseURI);
            client.DefaultRequestHeaders
                  .Accept
                  .Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync(url);
          
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();

            return jsonResponse;
        }

        private async Task<string> GetBook()
        {
            Random rnd = new Random();

            var json = await GetJSON(infoPostfix);

            Books books = JsonSerializer.Deserialize<Books>(json) ?? new Books();

            booksList.Clear();
            foreach ( Book book in books.books ) { 
                booksList.Add(book.abbreviation);
            }
            

            int bookNum = rnd.Next(books.books.Length);

            string abbrev = books.books[bookNum].abbreviation;

            // Pasowałoby wygenerować tego młodzieńca do Combosa
            cmbBook.SelectedValue = abbrev;
            return abbrev;

        }

        private async Task<string> GetChapter(string bookAbbrev)
        {
            Random rnd = new Random();

            var json = await GetJSON(infoPostfix + '/' + bookAbbrev);
            Book book = JsonSerializer.Deserialize<Book>(json);

            chaptersList.Clear();
            for( int i = 0; i <= book.chapters; i++)
            {
                chaptersList.Add(i+1);
            }

            int chaptersNum = (int)book.chapters;

            int randomChapter = rnd.Next(chaptersNum)+1;
            cmbChapter.SelectedValue = randomChapter;
            
            return randomChapter.ToString();
        }

        private async Task<string> GetVerses(string bookAbbrev, string chapterNum)
        {
            Random rnd = new Random();

            var json = await GetJSON(basePostfix + '/' + bookAbbrev + '/' + chapterNum);
            Chapter chapter = JsonSerializer.Deserialize<Chapter>(json);

            // To będzie podawane przez użytkownika 
            int len = chapter.verses.Length;

            allVersesList.Clear();
            for (int i = 0; i < len; i++)
            {
                allVersesList.Add(i + 1);
            }
            

            int randomVerse = rnd.Next(len) + 1;
            cmbStart.SelectedValue = randomVerse;

            int secondVerse = rnd.Next(len) + 1;
            
            secondVerse += randomVerse;
            if (secondVerse > len)
            {
                secondVerse = len;
            }
            
            selectableVersesList.Clear();
            for (int i = randomVerse; i <= len; i++)
            {
                selectableVersesList.Add(i);
            }

            cmbEnd.SelectedValue = secondVerse;

            string result = "";
            foreach (var verse in chapter.verses)
            {
                result += verse.text;
            }

            return result;
        }

        private async void GetBibleVerse()
        {
            string book = await GetBook();

            string chapter = await GetChapter(book);

            string verses = await GetVerses(book, chapter);

            blck_cite.Text = verses;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GetBibleVerse();
        }
    }

}
