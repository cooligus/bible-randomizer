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

namespace BibleRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        const string baseURI = "https://www.biblia.info.pl";
        const string infoPostfix = "/api/info/bt";
        const string basePostfix = "/api/biblia/bt";

        bool error = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async Task<string> GetJSON(string url)
        {
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

            Books books = JsonSerializer.Deserialize<Books>(json);

            int bookNum = rnd.Next(books.books.Length) + 1;

            string abbrev = books.books[bookNum].abbreviation;

            return abbrev;
        }

        private async Task<string> GetChapter(string bookAbbrev)
        {
            Random rnd = new Random();

            var json = await GetJSON(infoPostfix + '/' + bookAbbrev);
            Book book = JsonSerializer.Deserialize<Book>(json);

            int chaptersNum = (int)book.chapters;

            int randomChapter = rnd.Next(chaptersNum) + 1;

            return randomChapter.ToString();
        }

        private async Task<string> GetVerses(string bookAbbrev, string chapterNum)
        {
            Random rnd = new Random();

            var json = await GetJSON(basePostfix + '/' + bookAbbrev + '/' + chapterNum);
            Chapter chapter = JsonSerializer.Deserialize<Chapter>(json);

            int len = chapter.verses.Length;

            int randomVerse = rnd.Next(len) + 1;
            int secondVerse = rnd.Next(len);
            secondVerse += randomVerse;
            if (secondVerse > len)
            {
                secondVerse = len;
            }

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