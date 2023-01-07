using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public class OneCommeFetcher: IFetcher
{
    private const string COMMENT_JSON_FILE_NAME = "comment.json";
    private readonly string _oneCommeUrl;
    private readonly Uri _commentJsonUrl;

    private JsonObject[] _comments = Array.Empty<JsonObject>();
    public JsonObject[] Comments => _comments;

    public OneCommeFetcher(string oneCommeUrl)
    {
        _oneCommeUrl = oneCommeUrl;
        var baseUri = new Uri(oneCommeUrl);
        _commentJsonUrl = new Uri(baseUri, COMMENT_JSON_FILE_NAME);
    }

    public async Task Fetch(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_oneCommeUrl))
            return;
        
        var httpClient = new HttpClient();
        while (!token.IsCancellationRequested)
        {
            var responseMessage = await httpClient.GetAsync(_commentJsonUrl, token);
            var commentJsonObject =
                JsonSerializer.Deserialize<JsonObject>(await responseMessage.Content.ReadAsStringAsync(token));
            _comments = commentJsonObject["comments"].AsArray()
                .Select(jsonObject => jsonObject.AsObject())
                .ToArray();
            
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}