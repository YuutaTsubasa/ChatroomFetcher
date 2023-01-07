using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Writer;

public class OneCommeWriter : IWriter
{
    private const string EXTENDED_COMMENT_JSON_FILE_NAME = "extendedComment.json";
    private readonly string _templatePath;
    private readonly string _extendedCommentJsonFilePath;
    
    public OneCommeWriter(string templatePath)
    {
        _templatePath = templatePath;
        _extendedCommentJsonFilePath = Path.Combine(templatePath, EXTENDED_COMMENT_JSON_FILE_NAME);
    }
    
    public async Task Write(JsonObject[] comments)
    {
        if (string.IsNullOrEmpty(_templatePath))
            return;

        await File.WriteAllTextAsync(_extendedCommentJsonFilePath,
            JsonSerializer.Serialize(new JsonObject(new []
            {
                new KeyValuePair<string, JsonNode?>("comments", 
                    new JsonArray(comments.Select(comment => 
                        JsonNode.Parse(comment.ToJsonString())).ToArray()))
            })));
    }
}