using OversharingChecker.Api.Models;

namespace OversharingChecker.Api.Data;

/// <summary>
/// Provides the files and their permissions to scan. Today this is a JSON file;
/// a Microsoft Graph implementation can be added later without changing the rest of the app.
/// </summary>
public interface ISharingDataSource
{
    Task<IReadOnlyList<SharedItem>> GetItemsAsync(CancellationToken cancellationToken = default);
}
