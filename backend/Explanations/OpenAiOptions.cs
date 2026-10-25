namespace OversharingChecker.Api.Explanations;

/// <summary>
/// Settings for the "OpenAI" configuration section. Values come from user secrets or environment
/// variables (for example OpenAI__ApiKey) - never from files committed to Git.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string? ApiKey { get; set; }

    /// <summary>Model name for OpenAI, or the deployment name for Azure OpenAI.</summary>
    public string Model { get; set; } = "gpt-4.1-mini";

    /// <summary>Set to https://&lt;resource&gt;.openai.azure.com to use Azure OpenAI instead of OpenAI.</summary>
    public string? AzureEndpoint { get; set; }

    public string AzureApiVersion { get; set; } = "2024-10-21";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ApiKey);

    public bool UseAzure => !string.IsNullOrWhiteSpace(AzureEndpoint);
}
