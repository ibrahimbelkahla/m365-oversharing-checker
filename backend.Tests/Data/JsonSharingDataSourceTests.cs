using OversharingChecker.Api.Data;
using OversharingChecker.Api.Models;

namespace OversharingChecker.Tests.Data;

public class JsonSharingDataSourceTests
{
    private static readonly string SampleDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "sample-data.json");

    [Fact]
    public async Task Loads_all_sample_items()
    {
        var items = await new JsonSharingDataSource(SampleDataPath).GetItemsAsync();

        Assert.InRange(items.Count, 40, 60);
        Assert.All(items, item => Assert.False(string.IsNullOrWhiteSpace(item.Name)));
    }

    [Fact]
    public async Task Sample_data_contains_every_location_and_label()
    {
        var items = await new JsonSharingDataSource(SampleDataPath).GetItemsAsync();

        Assert.Equal(Enum.GetValues<LocationType>().Length, items.Select(i => i.LocationType).Distinct().Count());
        Assert.Equal(Enum.GetValues<SensitivityLabel>().Length, items.Select(i => i.SensitivityLabel).Distinct().Count());
    }
}
