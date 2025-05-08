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
        private readonly IHttpClientFactory _httpClientFactory;

        public ChatGPTClientController(
            ILogger<ChatGPTClientController> logger,
            Secrets secrets,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _secrets = secrets;
            _httpClientFactory = httpClientFactory;
        }


        [HttpGet(Name = "Get")]
        public async Task<IActionResult> Get()
        {
			var rawJson = @"
				[
					{
						""sectionId"": 1,
						""section"": ""SampleLocation\MachineType\MachileClassA"",
						""building"": """",
						""machine"": ""MachineModelA"",
						""childMachine"": ""MachineModelA"",
						""childMachineId"": 986,
						""availability"": 84.47,
						""performance"": 91.59,
						""quality"": 97.9903536977492,
						""oee"": 76,
						""reportDate"": ""2025-05-05T00:00:00"",
						""remarks"": ""Data Found"",
						""smartorManual"": ""Smart"",
						""machineTypeId"": 0
					},
					{
						""sectionId"": 1,
						""section"": ""SampleLocation\MachineType\MachileClassB"",
						""building"": """",
						""machine"": ""MachineModelB"",
						""childMachine"": ""MachineModelB"",
						""childMachineId"": 987,
						""availability"": 100,
						""performance"": 87.63,
						""quality"": 96.97406340057637,
						""oee"": 85,
						""reportDate"": ""2025-05-05T00:00:00"",
						""remarks"": ""Data Found"",
						""smartorManual"": ""Smart"",
						""machineTypeId"": 0
					},
					{
						""sectionId"": 1,
						""section"": ""SampleLocation\MachineType\MachileClassC"",
						""building"": """",
						""machine"": ""MachineModelC"",
						""childMachine"": ""MachineModelC"",
						""childMachineId"": 988,
						""availability"": 87.74,
						""performance"": 71.55,
						""quality"": 96.97885196374622,
						""oee"": 61,
						""reportDate"": ""2025-05-05T00:00:00"",
						""remarks"": ""Data Found"",
						""smartorManual"": ""Smart"",
						""machineTypeId"": 0
					}
				]";
            
			
		    var apiKey = _secrets.ApiKey;
            var apiUrl = _secrets.Endpoint;

            var analysisPrompt = @"
                You are an expert manufacturing analyst. Analyze the following OEE data and write a plain-language summary of key insights.
                Focus on trends, anomalies, underperforming machines, or patterns in availability, performance, quality, and OEE.
                Do not return a table — just explain in a paragraph with bold point what stands out in maximum 10 lines.

            ";

            var machineDataList = JsonSerializer.Deserialize<List<JsonElement>>(rawJson);
            var machineChunks = ChunkJson(machineDataList, chunkSize: 50);
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var combinedResponse = new StringBuilder();

            foreach (var chunk in machineChunks)
            {
                var chunkJson = JsonSerializer.Serialize(chunk);
                var fullPrompt = analysisPrompt + "\n\nData:\n" + chunkJson;

                var completion = await CallOpenAIAsync(apiUrl, httpClient, fullPrompt);
                if (completion == null)
                {
                    return StatusCode(500, "Failed to get response from OpenAI.");
                }

                combinedResponse.AppendLine(completion);
                await Task.Delay(14000); // Rate limit pause
            }

            // SECOND PROMPT
            var secondPrompt = @"
                Based on the following manufacturing insights, suggest 3 specific and actionable recommendations
                to improve overall equipment effectiveness (OEE). Keep them clear and practical:
            ";

            var finalPrompt = secondPrompt + "\n\nInsights:\n" + combinedResponse.ToString();

            var secondResponse = await CallOpenAIAsync(apiUrl, httpClient, finalPrompt);
            if (secondResponse == null)
            {
                return StatusCode(500, "Second GPT call failed.");
            }

            return Ok(new
            {
                InitialSummary = combinedResponse.ToString(),
                Recommendations = secondResponse
            });
        }

        private async Task<string?> CallOpenAIAsync(string apiUrl, HttpClient httpClient, string prompt)
        {
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a data analyst helping interpret machine efficiency reports." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 1000,
                temperature = 0.7
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync(apiUrl, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API error: {Error}", responseContent);
                return null;
            }

            try
            {
                var doc = JsonDocument.Parse(responseContent);
                return doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            catch (JsonException ex)
            {
                _logger.LogError("Parsing error: {Message}", ex.Message);
                return null;
            }
        }

        private List<List<JsonElement>> ChunkJson(List<JsonElement> dataList, int chunkSize)
        {
            var chunks = new List<List<JsonElement>>();
            for (int i = 0; i < dataList.Count; i += chunkSize)
            {
                chunks.Add(dataList.GetRange(i, Math.Min(chunkSize, dataList.Count - i)));
            }
            return chunks;
        }


    }
}
