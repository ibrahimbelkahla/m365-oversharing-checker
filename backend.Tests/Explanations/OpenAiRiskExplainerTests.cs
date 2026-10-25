using System.Net;
using System.Text;
using System.Text.Json;
using OversharingChecker.Api.Explanations;
using OversharingChecker.Api.Models;
using OversharingChecker.Api.Risk;
using static OversharingChecker.Tests.TestData;

namespace OversharingChecker.Tests.Explanations;

public class OpenAiRiskExplainerTests
{
    private const string AiAnswer = """{"explanation":"Too open.","businessRisk":"Data leak.","fixSteps":["Remove link","Add expiry"]}""";

    private static readonly RiskAssessment Assessment = new(
        Item(SensitivityLabel.HighlyConfidential, AnyoneLink(PermissionRole.Write), External(PermissionRole.Write)),
        [new RiskFinding("anyone-edit-link", Severity.Critical, "Anonymous link with edit rights", "reason with guest@partner.example")],
        80,
        Severity.Critical);

    /// <summary>Records the request and answers like the Chat Completions API would.</summary>
    private sealed class FakeHandler(HttpStatusCode status = HttpStatusCode.OK) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string RequestBody { get; private set; } = "";

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = await request.Content!.ReadAsStringAsync(cancellationToken);
            var response = JsonSerializer.Serialize(new { choices = new[] { new { message = new { role = "assistant", content = AiAnswer } } } });
            return new HttpResponseMessage(status) { Content = new StringContent(response, Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task Calls_OpenAI_with_bearer_token_and_parses_the_answer()
    {
        var handler = new FakeHandler();
        var explainer = new OpenAiRiskExplainer(new HttpClient(handler), new OpenAiOptions { ApiKey = "test-key", Model = "test-model" });

        var result = await explainer.ExplainAsync(Assessment);

        Assert.Equal("https://api.openai.com/v1/chat/completions", handler.Request!.RequestUri!.ToString());
        Assert.Equal("Bearer test-key", handler.Request.Headers.Authorization!.ToString());
        Assert.Contains("\"model\":\"test-model\"", handler.RequestBody);
        Assert.Equal("Too open.", result.Explanation);
        Assert.Equal(["Remove link", "Add expiry"], result.FixSteps);
        Assert.Equal("OpenAI (test-model)", result.GeneratedBy);
    }

    [Fact]
    public async Task Calls_Azure_OpenAI_deployment_with_api_key_header()
    {
        var handler = new FakeHandler();
        var explainer = new OpenAiRiskExplainer(new HttpClient(handler), new OpenAiOptions
        {
            ApiKey = "azure-key", Model = "my-deployment", AzureEndpoint = "https://example-resource.openai.azure.com/"
        });

        await explainer.ExplainAsync(Assessment);

        Assert.StartsWith("https://example-resource.openai.azure.com/openai/deployments/my-deployment/chat/completions?api-version=",
            handler.Request!.RequestUri!.ToString());
        Assert.Equal("azure-key", handler.Request.Headers.GetValues("api-key").Single());
        Assert.Null(handler.Request.Headers.Authorization);
    }

    [Fact]
    public async Task Only_metadata_is_sent_never_email_addresses()
    {
        var handler = new FakeHandler();
        var explainer = new OpenAiRiskExplainer(new HttpClient(handler), new OpenAiOptions { ApiKey = "k" });

        await explainer.ExplainAsync(Assessment);

        Assert.DoesNotContain("@", handler.RequestBody);
        Assert.Contains("Highly Confidential", handler.RequestBody);
        Assert.Contains("External guests: 1", handler.RequestBody);
    }

    [Fact]
    public async Task Http_error_is_reported_as_exception()
    {
        var explainer = new OpenAiRiskExplainer(new HttpClient(new FakeHandler(HttpStatusCode.Unauthorized)), new OpenAiOptions { ApiKey = "bad" });

        await Assert.ThrowsAsync<HttpRequestException>(() => explainer.ExplainAsync(Assessment));
    }
}
