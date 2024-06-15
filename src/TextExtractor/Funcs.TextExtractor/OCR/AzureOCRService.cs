using Funcs.TextExtractor.Configuration;
using Funcs.TextExtractor.Data.Dtos;
using Funcs.TextExtractor.Data.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace Funcs.TextExtractor.OCR;

public class AzureOCRService(IOptions<FunctionSettings> options, IHttpClientFactory clientFactory) : IOCRService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    private readonly string _endpoint = options.Value.AzAiServicesEndpoint;
    private readonly string _apiKey = options.Value.AzAiServicesApiKey;

    public async Task<ImageOCRResults> ExtractTextFromImageAsync(string imageUrl)
    {
        var client = _clientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/computervision/imageanalysis:analyze?api-version=2024-02-01&features=read&language=en");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

        //var content = new StringContent("{\"url\": \"https://learn.microsoft.com/azure/ai-services/computer-vision/media/quickstarts/presentation.png\"}", null, "application/json");
        //var content = new StringContent("{\"url\": \"https://sttextextractor.blob.core.windows.net/incoming-images/Note.jpg\"}", null, "application/json");
        var jsonPayload = $"{{\"url\": \"{imageUrl}\"}}";
        var content = new StringContent(jsonPayload, System.Text.Encoding.UTF8, "application/json");

        request.Content = content;
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            StringBuilder plainText = new(1024);

            // Parse the response to extract the text from the image
            // Deserialize JSON string to ImageAnalysisResult
            ImageAnalysisResult analysisResult = JsonConvert.DeserializeObject<ImageAnalysisResult>(result);
            foreach (var block in analysisResult.ReadResult.Blocks)
            {
                foreach (var line in block.Lines)
                {
                    plainText.Append($"{line.Text}{Environment.NewLine}");
                }
            }

            //Console.WriteLine($"Extracted Text: {plainText.ToString()}");

            return new ImageOCRResults { OCRResult = result, ExtractedText = plainText.ToString() };
        }
        else
        {
            throw new Exception($"Failed to extract text from image. Status code: {response.StatusCode}");
        }
    }
}