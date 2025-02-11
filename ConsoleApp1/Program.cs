using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

class Program
{
    static List<Repository> cachedRepositories = new List<Repository>();

    static async Task Main()
    {
        await pobieranieAPI();

        while (true)
        {
            Console.WriteLine("\nWybierz tryb:");
            Console.WriteLine("1 - Wszystkie repozytroia");
            Console.WriteLine("2 - Top 10/50/100 ");
            Console.WriteLine("3 - Repozytoria od daty");
            Console.WriteLine("4 - Wyswietl wedlug jezyka");
            Console.WriteLine("5 - Exit");

            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    wyswAll();
                    break;
                case "2":
                    wyswTop();
                    break;
                case "3":
                    wyswData();
                    break;
                case "4":
                    wyswJezyk();
                    break;
                case "5":
                    return;
                default:
                    Console.WriteLine("Zly wybor.");
                    break;
            }
        }
    }

    static async Task pobieranieAPI()
    {
        string url = "https://api.github.com/search/repositories?q=created:>2019-01-10&sort=stars&order\r\n=desc";

        using HttpClient client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "CSharp-App");

        try
        {
            string jsonResponse = await client.GetStringAsync(url);
            var response = JsonSerializer.Deserialize<GitHubResponse>(jsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response?.Items != null)
            {
                cachedRepositories = response.Items
                    .Select(repo => new Repository
                    {
                        name = repo.name,
                        stargazers_count = repo.stargazers_count,
                        language = repo.language ?? "Nieznany",
                        created_at = repo.created_at
                    })
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Blad przy pobieraniu z API: {ex.Message}");
        }
    }


    static void wyswAll()
    {
        if (!cachedRepositories.Any())
        {
            Console.WriteLine("Brak danych.");
            return;
        }

        Console.WriteLine("\nWszystkie repozytoria:\n");

        foreach (var repo in cachedRepositories)
        {
            Console.WriteLine($"{repo.name} - {repo.stargazers_count}*");
        }
    }

    static void wyswTop()
    {
        if (!cachedRepositories.Any())
        {
            Console.WriteLine("Brak danych.");
            return;
        }

        Console.WriteLine("\nWybierz TOP 10/50/100:");
        Console.WriteLine("A - Top 10");
        Console.WriteLine("B - Top 50");
        Console.WriteLine("C - Top 100");

        string topChoice = Console.ReadLine()?.ToUpper();
        int limit = topChoice switch
        {
            "A" => 10,
            "B" => 50,
            "C" => 100,
            _ => 10
        };

        var topRepos = cachedRepositories.Take(limit);
        Console.WriteLine($"\nTop {limit} repozytoriow:\n");

        foreach (var repo in topRepos)
        {
            Console.WriteLine($"{repo.name} - {repo.stargazers_count}*");
        }
    }

    static void wyswData()
    {
        Console.WriteLine("\nOd jakiej daty (YYYY-MM-DD) wyswietlic repozytoria:");
        string dateInput = Console.ReadLine();

        if (!DateTime.TryParse(dateInput, out DateTime selectedDate))
        {
            Console.WriteLine("Nieprawidlowy format daty.");
            return;
        }

        var filteredRepos = cachedRepositories
            .Where(repo => repo.created_at >= selectedDate)
            .ToList();

        if (!filteredRepos.Any())
        {
            Console.WriteLine($"Brak repozytoriow utworzonych po {selectedDate:yyyy-MM-dd}.");
            return;
        }

        Console.WriteLine($"\nRepozytoria utworzone po {selectedDate:yyyy-MM-dd}:\n");
        foreach (var repo in filteredRepos)
        {
            Console.WriteLine($"{repo.name} - {repo.created_at:yyyy-MM-dd} - {repo.stargazers_count}*");
        }
    }

    static void wyswJezyk()
    {
        Console.WriteLine("\nPodaj jezyk programowania:");
        string language = Console.ReadLine();

        var languageFilteredRepos = cachedRepositories
            .Where(repo => repo.language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!languageFilteredRepos.Any())
        {
            Console.WriteLine($"Brak repozytoriow w jezyku {language}.");
            return;
        }

        Console.WriteLine($"\nRepozytoria w jezyku {language}:\n");
        foreach (var repo in languageFilteredRepos)
        {
            Console.WriteLine($"{repo.name} - {repo.stargazers_count}*");
        }
    }

    public class GitHubResponse
    {
        public List<Repository> Items { get; set; }
    }

    public class Repository
    {
        public string name { get; set; }
        public int stargazers_count { get; set; }
        public string language { get; set; }
        public DateTime created_at { get; set; }
    }
}

