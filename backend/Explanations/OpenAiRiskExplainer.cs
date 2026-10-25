using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using OversharingChecker.Api.Risk;

namespace OversharingChecker.Api.Explanations;

/// <summary>
/// Asks the OpenAI (or Azure OpenAI) Chat Completions API to explain an item's risks.
/// Only metadata is sent - see <see cref="ExplanationPrompt"/>.
/// </summary>
public sealed class OpenAiRiskExplainer(HttpClient httpClient, OpenAiOptions options) : IRiskExplainer
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public string Name => options.UseAzure ? $"Azure OpenAI ({options.Model})" : $"OpenAI ({options.Model})";

    public async Task<RiskExplanation> ExplainAsync(RiskAssessment assessment, CancellationToken cancellationToken = default)
    {
        var body = new ChatRequest(
            options.Model,
            [
                new ChatMessage("system", ExplanationPrompt.SystemMessage),
                new ChatMessage("user", ExplanationPrompt.BuildUserMessage(assessment))
            ],
            new ResponseFormat("json_object"),
            Temperature: 0.3);

        using var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl()) { Content = JsonContent.Create(body) };
        if (options.UseAzure)
            request.Headers.Add("api-key", options.ApiKey);
        else
            request.Headers.Authorization = new("Bearer", options.ApiKey);

        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var chat = await response.Content.ReadFromJsonAsync<ChatResponse>(Json, cancellationToken);
        var content = chat?.Choices.FirstOrDefault()?.Message.Content
            ?? throw new InvalidOperationException("The AI response contained no message.");

        var answer = JsonSerializer.Deserialize<AiAnswer>(content, Json)
            ?? throw new InvalidOperationException("The AI response was not valid JSON.");

        return new RiskExplanation(answer.Explanation, answer.BusinessRisk, answer.FixSteps, Name);
    }

    private string BuildUrl() => options.UseAzure
        ? $"{options.AzureEndpoint!.TrimEnd('/')}/openai/deployments/{options.Model}/chat/completions?api-version={options.AzureApiVersion}"
        : "https://api.openai.com/v1/chat/completions";

    // --- Request and response shapes of the Chat Completions API (only the fields we use).

    private sealed record ChatRequest(
        string Model,
        List<ChatMessage> Messages,
        [property: JsonPropertyName("response_format")] ResponseFormat ResponseFormat,
        double Temperature);

    private sealed record ChatMessage(string Role, string Content);

    private sealed record ResponseFormat(string Type);

    private sealed record ChatResponse(List<ChatChoice> Choices);

    private sealed record ChatChoice(ChatMessage Message);

    // The JSON object we asked the model to produce.
    private sealed record AiAnswer(string Explanation, string BusinessRisk, List<string> FixSteps);
}
