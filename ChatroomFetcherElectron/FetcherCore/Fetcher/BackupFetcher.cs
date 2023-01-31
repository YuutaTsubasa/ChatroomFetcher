using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public class BackupFetcher: IFetcher
{
    private readonly string _backupFilePath;

    private JsonObject[] _comments = Array.Empty<JsonObject>();
    public JsonObject[] Comments => _comments;

    public BackupFetcher(string backupFilePath)
    {
        if (string.IsNullOrEmpty(backupFilePath))
            return;
        
        _backupFilePath = backupFilePath;
    }

    public async Task Fetch(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_backupFilePath))
            return;
        
        var backupContent = await File.ReadAllTextAsync(_backupFilePath, token);
        var commentJsonObject =
            JsonSerializer.Deserialize<JsonObject>(backupContent);
        _comments = commentJsonObject["comments"].AsArray()
            .Select(jsonObject => jsonObject.AsObject())
            .ToArray();
    }
}