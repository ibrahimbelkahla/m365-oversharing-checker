using System.Text.Json;
using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Data;

/// <summary>Reads Graph-shaped sample data from a JSON file. The file is read once and cached.</summary>
public sealed class JsonSharingDataSource(string filePath) : ISharingDataSource
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private IReadOnlyList<SharedItem>? _cache;

    public async Task<IReadOnlyList<SharedItem>> GetItemsAsync(CancellationToken cancellationToken = default)
    {
        if (_cache is not null) return _cache;

        await using var stream = File.OpenRead(filePath);
        var response = await JsonSerializer.DeserializeAsync<GraphResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException($"Sample data file '{filePath}' is empty.");

        _cache = response.Value.Select(item => GraphItemMapper.ToSharedItem(item, response.Tenant)).ToList();
        return _cache;
    }
}
