using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;

namespace wordFromNews;

class Program
{
    private static string apiKey = "6ceb011f5ad441909f6e7ab1a20fcee4";
    private static string topic = "космос";
    private static string section = "all";
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("Enter your API key from https://newsapi.org:");
        apiKey = Console.ReadLine();
        if(string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("The API key cannot be null or an empty string.");
            return;
        }

        Console.WriteLine("Enter a topic to search for:");
        topic = Console.ReadLine() ?? topic;

        Console.WriteLine("Enter a section to search for (author, title, description, content):");
        section = Console.ReadLine() ?? section;
        
        var newsArticles = await GetNews(topic);

        foreach (var article in newsArticles)
        {
            if (!string.IsNullOrEmpty(article.Content))
                continue;
            
            string maxGlossaryWord = FindMaxGlossaryWord(article.Content);
            Console.WriteLine($"News content: {article.Content}");
            Console.WriteLine($"Max glossary word: {maxGlossaryWord}");
            Console.WriteLine();
            Console.ReadLine();
            
        }
    }
    /// <summary>
    /// метод получает новости с newsApi
    /// использует библиотеку NewsApi
    /// Стандартным httpclient часто выдает ошибку "Bad Request"
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    private static async Task<List<Article>> GetNews(string topic)
    {
        var newsApiClient = new NewsApiClient(apiKey);
        var articles = new List<Article>();

        
        var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
        {
            Q = topic,
            SortBy = SortBys.Popularity,
            Language = DetectLanguage(topic),
        });

        if (articlesResponse.Status == Statuses.Ok)
        {
            foreach (var article in articlesResponse.Articles)
            {
                articles.Add(article);
            }
        }
        return articles;
    }

    /// <summary>
    /// Поиск максимального слова в фрагменте текста
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    private static string FindMaxGlossaryWord(string text)
    {
        string[] words = text.Split(new char[] { ' ', '.', ',', ';', ':', '\'', '\"', '!', '?', '(', ')', '[', ']', '{', '}', '-', '_', '/' },
            StringSplitOptions.RemoveEmptyEntries);

        return words.OrderByDescending(word => CountGlossary(word)).FirstOrDefault();
    }

    private static int CountGlossary(string word)
    {
        int glossaryCount = 0;
        string glossary = "aeiouаиеёоуыэюя";

        foreach (var character in word.ToLower())
        {
            if (glossary.IndexOf(character) >= 0)
            {
                glossaryCount++;
            }
        }

        return glossaryCount;
    }
    /// <summary>
    /// Определение языка ввода текста 
    /// </summary>
    /// <param name="topic"></param>
    /// <returns></returns>
    private static Languages DetectLanguage(string topic)
    {
        int EN_count = 0, RU_count = 0;
        byte[] bytes = System.Text.Encoding.Default.GetBytes(topic);
        foreach (byte bt in bytes)
        {
            if ((bt >= 97) && (bt <= 122)) EN_count++;
            if ((bt >= 192) && (bt <= 239)) RU_count++;
        }
        if (EN_count > RU_count) return Languages.EN;
        if (EN_count < RU_count) return Languages.RU;
        return Languages.EN;//универсальным пусть будет EN
    }
}

