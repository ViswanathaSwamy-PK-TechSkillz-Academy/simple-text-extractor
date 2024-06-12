﻿using FuncApp_TextExtractor.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace FuncApp_TextExtractor.OCR;

public class AzureComputerVisionOCRService(IOptions<FunctionSettings> options, IHttpClientFactory clientFactory) : IOCRService
{
    private readonly IHttpClientFactory _clientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
    private readonly string _endpoint = options.Value.AzAiServicesEndpoint;
    private readonly string _apiKey = options.Value.AzAiServicesApiKey;

    public async Task<string> ExtractTextFromImageAsync(string imageUrl)
    {
        var client = _clientFactory.CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_endpoint}/computervision/imageanalysis:analyze?api-version=2024-02-01&features=read&language=en");
        request.Headers.Add("Ocp-Apim-Subscription-Key", _apiKey);

        var content = new StringContent("{\"url\": \"https://learn.microsoft.com/azure/ai-services/computer-vision/media/quickstarts/presentation.png\"}", null, "application/json");

        request.Content = content;
        var response = await client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            // Parse the response to extract the text from the image
            // For simplicity, let's assume the text is directly returned in the response
            return result;
        }
        else
        {
            throw new Exception($"Failed to extract text from image. Status code: {response.StatusCode}");
        }
    }
}