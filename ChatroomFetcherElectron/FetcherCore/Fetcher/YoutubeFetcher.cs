using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatroomFetcherElectron.FetcherCore.Fetcher.Data;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public class YoutubeFetcher : IFetcher
{
    private const string LIVE_CHAT_API_URI_FORMAT =
        "https://www.youtube.com/live_chat?is_popout=1&v={0}";

    private const string GET_LIVE_CHAT_API_URI_FORMAT =
        "https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?key={0}&prettyPrint=false";

    private readonly string _liveId;
    private readonly Uri _liveChatApiUri;

    public JsonObject[] Comments { get; private set; }
        = Array.Empty<JsonObject>();

    public YoutubeFetcher(string liveId)
    {
        _liveId = liveId;
        _liveChatApiUri = new Uri(string.Format(LIVE_CHAT_API_URI_FORMAT, liveId));
    }
    
    public async Task Fetch(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_liveId))
            return;
        
        try
        {
            var cookieContainer = new CookieContainer();
            var httpClientHandler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Add(
                "user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36 Edg/108.0.1462.54");
            httpClient.DefaultRequestHeaders.Add(
                "origin",
                "https://www.youtube.com");

            var chatroomHtmlContent =
                System.Text.Encoding.UTF8.GetString(
                    await httpClient.GetByteArrayAsync(_liveChatApiUri, token));
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(chatroomHtmlContent);

            var contextNode =
                htmlDocument.DocumentNode
                    .QuerySelectorAll("script")
                    .First(node => node.InnerText.Contains("INNERTUBE_CONTEXT"));
            var extractYtConfigJsonRegEx = new Regex(@"ytcfg\.set\((.+)\);");
            var ytConfigJsonContent = extractYtConfigJsonRegEx.Matches(contextNode.InnerText)[0]
                .Groups[1]
                .Value;
            var ytCfgJsonObject = JsonSerializer.Deserialize<JsonObject>(ytConfigJsonContent);
            
            var apiKey = ytCfgJsonObject["INNERTUBE_API_KEY"].GetValue<string>();
            var innerTubeContext = ytCfgJsonObject["INNERTUBE_CONTEXT"];
            var clientData = innerTubeContext["client"].Deserialize<YoutubeChatRequestData.ClientData>(new JsonSerializerOptions
            {
                IncludeFields = true
            });
            clientData.AcceptHeader =
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            clientData.ConnectionType = "CONN_CELLULAR_4G";
            clientData.MainAppWebInfo = new YoutubeChatRequestData.MainAppWebInfoData
            {
                GraftUrl = _liveChatApiUri.AbsoluteUri,
                IsWebNativeShareAvailable = true,
                WebDisplayMode = "WEB_DISPLAY_MODE_BROWSER"
            };
            clientData.MemoryTotalKbytes = "8000000";
            clientData.ScreenDensityFloat = 1.100000023841858f;
            clientData.ScreenHeightPoints = 845;
            clientData.ScreenPixelDensity = 1;
            clientData.ScreenWidthPoints = 529;
            clientData.TimeZone = "Asia/Taipei";
            clientData.UserInterfaceTheme = "USER_INTERFACE_THEME_LIGHT";
            clientData.UtcOffsetMinutes = 480;

            var ytInitialData = htmlDocument.DocumentNode
                .QuerySelectorAll("script")
                .First(node => node.InnerText.Contains("window[\"ytInitialData\"]"));
            var extractYtInitialDataJsonRegEx = new Regex(@"window\[""ytInitialData""\]\ +\=\ +({.+});\s*");
            var ytInitialDataJsonContent = extractYtInitialDataJsonRegEx
                .Matches(ytInitialData.InnerText)[0]
                .Groups[1]
                .Value;
            var ytInitialDataJsonObject = JsonSerializer.Deserialize<JsonObject>(ytInitialDataJsonContent);
            var continuation =
                ytInitialDataJsonObject["contents"]["liveChatRenderer"]["continuations"][0][
                    "invalidationContinuationData"]["continuation"].GetValue<string>();

            var initialComments = _ConvertToComments(
                ytInitialDataJsonObject["contents"]["liveChatRenderer"]["actions"]
                    ?.AsArray()
                    .Where(jsonObject => jsonObject["addChatItemAction"] != null)
                    .Select(jsonObject => jsonObject["addChatItemAction"]["item"]["liveChatTextMessageRenderer"])
                    .Where(renderer => renderer != null)
                    .ToArray() ?? Array.Empty<JsonNode>());
            Comments = Comments.Concat(initialComments).ToArray();
            
            var requestData = new YoutubeChatRequestData
            {
                Context = new YoutubeChatRequestData.ContextData
                {
                    Client = clientData,
                    Request = new YoutubeChatRequestData.RequestData
                    {
                        UseSsl = true,
                        ConsistencyTokenJars = Array.Empty<string>(),
                        InternalExperimentFlags = Array.Empty<string>()
                    },
                    User = new YoutubeChatRequestData.UserData
                    {
                        LockedSafetyMode = false
                    }
                },
                Continuation = continuation,
                WebClientInfo = new YoutubeChatRequestData.WebClientInfoData
                {
                    IsDocumentHidden = false
                }
            };

            var getLiveChatApiUri = new Uri(string.Format(GET_LIVE_CHAT_API_URI_FORMAT, apiKey));
            while (true)
            {
                var result = await httpClient.PostAsJsonAsync(getLiveChatApiUri,
                    JsonSerializer.Deserialize<JsonObject>(JsonSerializer.Serialize(requestData,
                        new JsonSerializerOptions
                        {
                            IncludeFields = true
                        })), token);
                
                var contentText = await result.Content.ReadAsStringAsync(token);
                var contentJsonObject = JsonSerializer.Deserialize<JsonObject>(contentText);
                requestData.Continuation =
                    contentJsonObject["continuationContents"]["liveChatContinuation"]["continuations"][0]
                    ["invalidationContinuationData"]["continuation"].GetValue<string>();
                
                var addChatItemRenderers = contentJsonObject?["continuationContents"]?["liveChatContinuation"]?["actions"]
                    ?.AsArray()
                    .Where(jsonObject => jsonObject["addChatItemAction"] != null)
                    .Select(jsonObject => jsonObject["addChatItemAction"]["item"]["liveChatTextMessageRenderer"])
                    .Where(renderer => renderer != null)
                    .ToArray() ?? Array.Empty<JsonNode>();
                var comments = _ConvertToComments(addChatItemRenderers);
                Comments = Comments.Concat(comments).ToArray();

                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }

    private IEnumerable<JsonObject> _ConvertToComments(JsonNode?[] addChatItemRenderers) =>
        addChatItemRenderers.Select(renderer =>
        {
            var name = renderer["authorName"]["simpleText"].GetValue<string>();
            var profileImage = renderer["authorPhoto"]["thumbnails"][0]["url"].GetValue<string>();
            var badges = renderer["authorBadges"]?.AsArray()
                .Select(badge => badge["liveChatAuthorBadgeRenderer"])
                .Select(renderer => new JsonObject(new[]
                {
                    new KeyValuePair<string, JsonNode?>("label", renderer["icon"] != null
                        ? renderer["icon"]["iconType"].GetValue<string>()
                        : renderer["tooltip"].GetValue<string>()),
                    new KeyValuePair<string, JsonNode?>("url", renderer["customThumbnail"] != null
                        ? renderer["customThumbnail"]["thumbnails"][0]["url"].GetValue<string>()
                        : string.Empty)
                }))
                .ToArray() ?? Array.Empty<JsonObject>();
            
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("id", $"youtube"),
                new KeyValuePair<string, JsonNode?>("service", "youtube"),
                new KeyValuePair<string, JsonNode?>("name", "Youtube 訊息"),
                new KeyValuePair<string, JsonNode?>("url", $"https://youtu.be/{_liveId}"),
                new KeyValuePair<string, JsonNode?>("color", new JsonObject(new[]
                {
                    new KeyValuePair<string, JsonNode?>("r", 255),
                    new KeyValuePair<string, JsonNode?>("g", 5),
                    new KeyValuePair<string, JsonNode?>("b", 5)
                })),
                new KeyValuePair<string, JsonNode?>("data", new JsonObject(new[]
                {
                    new KeyValuePair<string, JsonNode?>("id", renderer["id"].GetValue<string>()),
                    new KeyValuePair<string, JsonNode?>("liveId", _liveId),
                    new KeyValuePair<string, JsonNode?>("userId",
                        renderer["authorExternalChannelId"].GetValue<string>()),
                    new KeyValuePair<string, JsonNode?>("name", name),
                    new KeyValuePair<string, JsonNode?>("profileImage", profileImage),
                    // TODO: need to parse badge.
                    new KeyValuePair<string, JsonNode?>("badges", new JsonArray(badges)),
                    new KeyValuePair<string, JsonNode?>("isOwner", badges.Any(badge => badge["label"].GetValue<string>() == "OWNER")),
                    new KeyValuePair<string, JsonNode?>("isModerator", badges.Any(badge => badge["label"].GetValue<string>() == "MODERATOR")),
                    new KeyValuePair<string, JsonNode?>("isMember", badges.Any(badge => !string.IsNullOrEmpty(badge["url"].GetValue<string>()))),
                    new KeyValuePair<string, JsonNode?>("autoModerated", false),
                    // need to parse badge end.
                    new KeyValuePair<string, JsonNode?>("hasGift", false),
                    new KeyValuePair<string, JsonNode?>("comment", string.Join(string.Empty,
                        renderer["message"]["runs"].AsArray()
                            .Select(run => run.AsObject().ContainsKey("emoji")
                                ? $"<img src=\"{run["emoji"]["image"]["thumbnails"][0]["url"].GetValue<string>()}\"/>"
                                : run["text"].GetValue<string>()))),
                    new KeyValuePair<string, JsonNode?>("timestamp",
                        double.Parse(renderer["timestampUsec"].GetValue<string>()) / 1000.0),
                    new KeyValuePair<string, JsonNode?>("paidText", string.Empty),
                    new KeyValuePair<string, JsonNode?>("unit", string.Empty),
                    new KeyValuePair<string, JsonNode?>("price", string.Empty),
                    new KeyValuePair<string, JsonNode?>("colors", new JsonObject()),
                    new KeyValuePair<string, JsonNode?>("displayName", name),
                    new KeyValuePair<string, JsonNode?>("originalProfileImage", profileImage),
                    new KeyValuePair<string, JsonNode?>("isFirstTime", true)
                })),
                new KeyValuePair<string, JsonNode?>("meta", new JsonObject(new[]
                {
                    new KeyValuePair<string, JsonNode?>("interval", 0)
                }))
            });
        });
}