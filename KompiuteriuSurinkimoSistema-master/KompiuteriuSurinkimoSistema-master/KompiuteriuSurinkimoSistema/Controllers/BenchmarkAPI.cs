using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace ComputerBuildSystem.Controllers
{
    public class BenchmarkAPI:Controller
    {
        
        private readonly HttpClient _httpClient;

        public BenchmarkAPI(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Dictionary<string, int> CompareGPUStatus(List<string> deviceNames)
        {
            var result = new Dictionary<string, int>();
            var url = "https://browser.geekbench.com/opencl-benchmarks";

            var response = _httpClient.GetStringAsync(url).Result;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var rows = htmlDoc.DocumentNode.SelectNodes("//table[@class='table benchmark-chart-table']//tbody//tr");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var name = row.SelectSingleNode(".//td[@class='name']").InnerText.Trim();
                    var scoreTXT = row.SelectSingleNode(".//td[@class='score']").InnerText.Trim();

                    if (deviceNames.Contains(name))
                    {
                        int score;
                        if (int.TryParse(scoreTXT, out score))
                        {
                            result[name] = score;
                        }
                    }
                }
            }

            return result;
        }
        public int SendGPU(string deviceNames)
        {
            int highestScore = 0;
            var url = "https://browser.geekbench.com/opencl-benchmarks";

            var response = _httpClient.GetStringAsync(url).Result;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var rows = htmlDoc.DocumentNode.SelectNodes("//table[@class='table benchmark-chart-table']//tbody//tr");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var name = row.SelectSingleNode(".//td[@class='name']").InnerText.Trim();
                    var scoreTXT = row.SelectSingleNode(".//td[@class='score']").InnerText.Trim();

                    if (deviceNames.Contains(name))
                    {
                        if (int.TryParse(scoreTXT, out int score))
                        {
                            if (score > highestScore)
                            {
                                highestScore = score;
                            }
                        }
                    }
                }
            }

            return highestScore;
        }

        public int SendCPU(string deviceNames)
        {
            int highestScore = 0;
            var url = "https://browser.geekbench.com/processor-benchmarks";

            var response = _httpClient.GetStringAsync(url).Result;

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            var rows = htmlDoc.DocumentNode.SelectNodes("//table[@class='table benchmark-chart-table']//tbody//tr");

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    var name = row.SelectSingleNode(".//td[@class='name']").InnerText.Trim();
                    var scoreTXT = row.SelectSingleNode(".//td[@class='score']").InnerText.Trim();

                    if (name.Contains(deviceNames))
                    {
                        if (int.TryParse(scoreTXT, out int score))
                        {
                            if (score > highestScore)
                            {
                                highestScore = score;
                            }
                        }
                    }
                }
            }

            return highestScore;
        }
    }

}
