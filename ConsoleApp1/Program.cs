using ConsoleApp1.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Write("Enter folder path just in english: ");
        string folderPath = Console.ReadLine();

        Console.Write("Enter file format (JSON or CSV): ");
        string fileFormat = Console.ReadLine().ToLower();

        using var httpClient = new HttpClient();
        UserService userService = new UserService(httpClient);

        var apiUrls = new string[]
        {
            "https://randomuser.me/api/",
            "https://jsonplaceholder.typicode.com/users",
            "https://dummyjson.com/users",
            "https://reqres.in/api/users"
        };
        var allUsers = new List<TargetUser>();

        foreach (var apiUrl in apiUrls)
        {
            var usersFromApi = await userService.FetchUsersFromApi(apiUrl);
            allUsers.AddRange(usersFromApi);

            Console.WriteLine($"Users fetched from {apiUrl}: {usersFromApi.Count}");
        }

        Console.WriteLine($"Total users from all APIs: {allUsers.Count}");

        // Write data to a file
        WriteToFile(folderPath, fileFormat, allUsers);
    }

    static void WriteToFile(string folderPath, string fileFormat, List<TargetUser> users)
    {
        if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
        {
            Console.WriteLine("Invalid folder path.");
            return;
        }

        string filePath = Path.Combine(folderPath, $"users.{fileFormat}");

        try
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                if (fileFormat == "json")
                {
                    var json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
                    writer.Write(json);
                }
                else if (fileFormat == "csv")
                {
                    writer.WriteLine("First Name,Last Name,Email,Source ID");
                    foreach (var user in users)
                    {
                        writer.WriteLine($"{user.FirstName},{user.LastName},{user.Email},{user.SourceId}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid file format.");
                }
            }

            Console.WriteLine($"Data written to {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
        }
    }
}
