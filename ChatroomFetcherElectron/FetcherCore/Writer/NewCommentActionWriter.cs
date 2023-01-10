using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Writer;

public class NewCommentActionWriter : IWriter
{
    private HashSet<string> _existedCommentId = new HashSet<string>();
    private readonly Action<JsonObject[]> _actionHandler;

    public NewCommentActionWriter(Action<JsonObject[]> actionHandler)
    {
        _actionHandler = actionHandler;
    }
    
    public Task Write(JsonObject[] comments)
    {
        var newComments = comments
            .Where(comment => !_existedCommentId.Contains(comment["data"]["id"].GetValue<string>()))
            .ToArray();
        
        _actionHandler?.Invoke(newComments);
        _existedCommentId = new HashSet<string>(comments.Select(comment => comment["data"]["id"].GetValue<string>()));
        return Task.CompletedTask;
    }
}