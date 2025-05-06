using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using ChatGPTAPIClient.Models;

namespace ChatGPTAPIClient.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatGPTClientController : ControllerBase
    {
        private readonly ILogger<ChatGPTClientController> _logger;
        private readonly Secrets _secrets;

        public ChatGPTClientController(ILogger<ChatGPTClientController> logger, Secrets secrets)
        {
            _logger = logger;
            _secrets = secrets;
        }

        [HttpGet(Name = "Get")] //ChatGPTDefaultResponse
        public async Task<IActionResult> Get()
        {
            var apiKey = _secrets.ApiKey;
            var apiUrl = _secrets.Endpoint;


            var json = @"
                [
                  {
                    ""Sl"": 1,
                    ""Date"": ""23/04/2025"",
                    ""Section"": ""HIP> PML> Plastic Others"",
                    ""Machine"": ""HDPE Machine-7(B-14)"",
                    ""Building"": ""Building-14-(P)"",
                    ""Availability"": 95.35,
                    ""Performance"": 100,
                    ""Quality"": 99,
                    ""OEE"": 94,
                    ""Remarks"": """",
                    ""Type"": ""Smart""
                  },
                  {
                    ""Sl"": 2,
                    ""Date"": ""23/04/2025"",
                    ""Section"": ""HIP> PML> Plastic Others"",
                    ""Machine"": ""Blowing m/c-02 (B-08)"",
                    ""Building"": ""Building-8"",
                    ""Availability"": 83.77,
                    ""Performance"": 80.67,
                    ""Quality"": 99,
                    ""OEE"": 67,
                    ""Remarks"": """",
                    ""Type"": ""Smart""
                  },
                  {
                    ""Sl"": 3,
                    ""Date"": ""23/04/2025"",
                    ""Section"": ""HIP> PML> Plastic Others"",
                    ""Machine"": ""Bag Cutting m/c-02(B-14)"",
                    ""Building"": ""Building-14-(P)"",
                    ""Availability"": 80.81,
                    ""Performance"": 72.53,
                    ""Quality"": 91.46,
                    ""OEE"": 54,
                    ""Remarks"": """",
                    ""Type"": ""Smart""
                  }
                ]";



            var prompt = @"
                You are an expert manufacturing analyst. Analyze the following OEE data and write a plain-language summary of key insights.
                Focus on trends, anomalies, underperforming machines, or patterns in availability, performance, quality, and OEE.
                Do not return a table — just explain in a paragraph what stands out.
                ";
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                  {
            new { role = "system", content = "You are a data analyst helping interpret machine efficiency reports." },
            new { role = "user", content = prompt + "\n\nData:\n" + json }
        }
            };

            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            return Ok(responseContent);
        }
    }
}
