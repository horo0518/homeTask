using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ConsoleApp1.models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<List<TargetUser>> FetchUsersFromApi(string apiUrl)
    {
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        JToken responseObject = JToken.Parse(responseBody);

        JToken userArrayToken = GetArrayTokenFromResponse(responseObject, apiUrl);
        List<TargetUser> endUsers = ExtractUsersFromResponse(userArrayToken, apiUrl);

        PrintUserDetails(endUsers);

        return endUsers;
    }

    private JToken GetArrayTokenFromResponse(JToken response, string apiUrl)
    {
        switch (apiUrl)
        {
            case "https://randomuser.me/api/":
                return response["results"];
            case "https://jsonplaceholder.typicode.com/users":
                return response;
            case "https://dummyjson.com/users":
                return response["users"];
            case "https://reqres.in/api/users":
                return response["data"];
            default:
                return null;
        }
    }

    private List<TargetUser> ExtractUsersFromResponse(JToken userArrayToken, string sourceId)
    {
        List<TargetUser> users = new List<TargetUser>();

        if (userArrayToken is JArray userArray)
        {
            foreach (JToken userToken in userArray)
            {
                string firstName = ExtractFirstName(userToken);
                string lastName = ExtractLastName(userToken);
                string email = ExtractEmail(userToken);

                TargetUser user = new TargetUser
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    SourceId = sourceId
                };

                users.Add(user);
            }
        }

        return users;
    }

    private string ExtractFirstName(JToken userToken)
    {
        if (userToken is JObject userObject)
        {
            // Check if the "name" field is an object with "first" property
            var nameObject = userObject["name"];
            if (nameObject is JObject nameJsonObject)
            {
                return nameJsonObject["first"]?.ToString();
            }

            // If "name" is not an object, try to directly access the "last_name" property
            return userObject["name"]?.ToString() ?? userObject["first_name"]?.ToString() ?? userObject["firstName"]?.ToString();
        }

        return null;
    }


    private string ExtractLastName(JToken userToken)
    {
        if (userToken is JObject userObject)
        {
            // Check if the "name" field is an object with "last" property
            var nameObject = userObject["name"];
            if (nameObject is JObject nameJsonObject)
            {
                return nameJsonObject["last"]?.ToString();
            }

            // If "name" is not an object, try to directly access the "last_name" property
            return userObject["name"]?.ToString() ?? userObject["last_name"]?.ToString()??userObject["lastName"]?.ToString();
        }

        return null;
    }




    private string ExtractEmail(JToken userToken)
{
    if (userToken is JObject userObject)
    {
        return userObject["email"]?.ToString();
    }
    return null;
}

    private void PrintUserDetails(List<TargetUser> users)
    {
        foreach (var user in users)
        {
            Console.WriteLine($"First Name: {user.FirstName}");
            Console.WriteLine($"Last Name: {user.LastName}");
            Console.WriteLine($"Email: {user.Email}");
            Console.WriteLine($"Source ID: {user.SourceId}");
            Console.WriteLine("==============");
        }
    }
}
