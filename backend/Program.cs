using System.Text.Json.Serialization;
using OversharingChecker.Api.Data;
using OversharingChecker.Api.Explanations;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Risk.Rules;
using OversharingChecker.Api.Services;

const string FrontendCorsPolicy = "frontend";

var builder = WebApplication.CreateBuilder(args);

// --- Data source: sample JSON today, Microsoft Graph later (just register a different ISharingDataSource).
var sampleDataPath = Path.Combine(AppContext.BaseDirectory,
    builder.Configuration["SampleData:Path"] ?? Path.Combine("Data", "sample-data.json"));
builder.Services.AddSingleton<ISharingDataSource>(new JsonSharingDataSource(sampleDataPath));

// --- Risk engine: every registered IRiskRule is picked up by the RiskAnalyzer.
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IRiskRule, AnyoneLinkRule>();
builder.Services.AddSingleton<IRiskRule, ExternalGuestEditRule>();
builder.Services.AddSingleton<IRiskRule, ConfidentialExternalShareRule>();
builder.Services.AddSingleton<IRiskRule, OrganizationLinkOnConfidentialRule>();
builder.Services.AddSingleton<IRiskRule, StaleOrNonExpiringLinkRule>();
builder.Services.AddSingleton<IRiskRule>(new ExcessiveDirectSharingRule(
    builder.Configuration.GetValue("Risk:MaxDirectGrants", ExcessiveDirectSharingRule.DefaultMaxDirectGrants)));
builder.Services.AddSingleton<IRiskRule, AnyoneEditLinkRule>();
builder.Services.AddSingleton<RiskAnalyzer>();
builder.Services.AddSingleton<ScanService>();

// --- Explanations: use OpenAI / Azure OpenAI when a key is configured, otherwise the template fallback.
// The key is read from user secrets or environment variables (OpenAI__ApiKey or OPENAI_API_KEY).
var openAiOptions = builder.Configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>() ?? new OpenAiOptions();
if (string.IsNullOrWhiteSpace(openAiOptions.ApiKey))
    openAiOptions.ApiKey = builder.Configuration["OPENAI_API_KEY"];

if (openAiOptions.IsConfigured)
{
    builder.Services.AddSingleton(openAiOptions);
    builder.Services.AddHttpClient<IRiskExplainer, OpenAiRiskExplainer>(client => client.Timeout = TimeSpan.FromSeconds(30));
}
else
{
    builder.Services.AddSingleton<IRiskExplainer, RuleBasedRiskExplainer>();
}

// --- Web API: enums are sent as readable strings ("Critical" instead of 3).
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// Allow the Vite dev server to call the API from the browser.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:5173"];
builder.Services.AddCors(options => options.AddPolicy(FrontendCorsPolicy,
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // OpenAPI document at /openapi/v1.json
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "M365 Oversharing Checker API"));
}

app.UseCors(FrontendCorsPolicy);
app.MapControllers();

app.Run();

// Exposes the Program class so integration tests can start the API in memory.
public partial class Program { }
