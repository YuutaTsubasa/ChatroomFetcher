using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Writer;

public class BackupWriter : IWriter
{
    private const string LOG_DIRECTORY_NAME = "backup";
    private const string LOG_FILE_FORMAT = "Backup-{0}.json";
    private readonly string _backupFilePath;

    public BackupWriter()
    {
        _backupFilePath = Path.Combine(
            LOG_DIRECTORY_NAME,
            string.Format(LOG_FILE_FORMAT, DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")));
        Directory.CreateDirectory(LOG_DIRECTORY_NAME);
    }
    
    public async Task Write(JsonObject[] comments)
    {
        await File.WriteAllTextAsync(_backupFilePath,
            JsonSerializer.Serialize(new JsonObject(new []
            {
                new KeyValuePair<string, JsonNode?>("comments", 
                    new JsonArray(comments.Select(comment => 
                        JsonNode.Parse(comment.ToJsonString())).ToArray()))
            })));
    }
}