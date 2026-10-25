using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using OversharingChecker.Api.Risk;
using OversharingChecker.Api.Services;

namespace OversharingChecker.Tests.Api;

/// <summary>Starts the real API in memory and calls it over HTTP.</summary>
public class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/api/health");

        response.EnsureSuccessStatusCode();
        Assert.Contains("\"ok\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Items_are_sorted_by_score_and_use_string_enums()
    {
        var raw = await _client.GetStringAsync("/api/items");
        var items = JsonSerializer.Deserialize<List<RiskAssessment>>(raw, Json)!;

        Assert.NotEmpty(items);
        Assert.Equal(items.OrderByDescending(i => i.RiskScore).Select(i => i.RiskScore), items.Select(i => i.RiskScore));
        Assert.Contains("\"Critical\"", raw);
    }

    [Fact]
    public async Task Items_can_be_filtered()
    {
        var items = await _client.GetFromJsonAsync<List<RiskAssessment>>(
            "/api/items?severity=Critical&location=SharePoint&label=HighlyConfidential", Json);

        Assert.NotEmpty(items!);
        Assert.All(items!, i => Assert.Equal("Critical", i.HighestSeverity.ToString()));
        Assert.All(items!, i => Assert.Equal("SharePoint", i.Item.LocationType.ToString()));
    }

    [Fact]
    public async Task Single_item_is_returned_and_unknown_id_is_404()
    {
        var item = await _client.GetFromJsonAsync<RiskAssessment>("/api/items/aw-007", Json);
        var missing = await _client.GetAsync("/api/items/does-not-exist");

        Assert.Equal("Gehaltsliste-2026.xlsx", item!.Item.Name);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Summary_totals_add_up()
    {
        var summary = await _client.GetFromJsonAsync<ScanSummary>("/api/summary", Json);

        Assert.Equal(summary!.ItemsAtRisk, summary.ItemsBySeverity.Values.Sum());
        Assert.True(summary.ExternalShares > 0);
        Assert.InRange(summary.TopRiskySites.Count, 1, ScanService.TopSiteCount);
    }
}
