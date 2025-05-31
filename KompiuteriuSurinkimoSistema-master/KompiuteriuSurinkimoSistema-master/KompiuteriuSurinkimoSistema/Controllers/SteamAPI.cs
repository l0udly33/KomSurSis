using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ComputerBuildSystem.Controllers
{
    public static class SteamAPI
    {
        private static readonly HttpClient client = new HttpClient();

        public static string GetGameData(string appId)
        {
            string url = $"https://store.steampowered.com/api/appdetails?appids={appId}";

            try
            {
                HttpResponseMessage response = client.GetAsync(url).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;

                JObject json = JObject.Parse(responseBody);
                JObject gameData = (JObject)json[appId]["data"];

                if (gameData != null)
                {
                    JObject pcRequirements = (JObject)gameData["pc_requirements"];
                    if (pcRequirements != null)
                    {
                        string minimum = pcRequirements["minimum"]?.ToString();
                        return minimum;
                    }
                }

                return "No minimum requirements found.";
            }
            catch (Exception e)
            {
                return $"Unexpected error: {e.Message}";
            }
        }
    }
}
