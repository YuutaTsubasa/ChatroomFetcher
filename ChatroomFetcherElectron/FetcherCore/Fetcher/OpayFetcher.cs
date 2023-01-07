using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using ChatroomFetcherElectron.FetcherCore.Fetcher.Data;
using ChatroomFetcherElectron.FetcherCore.Utility;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public class OpayFetcher : IFetcher
{
    private const string OPAY_ANIMATION_URL_FORMAT =
        "https://payment.opay.tw/Broadcaster/AlertBox/{0}";
    private const string OPAY_CHECK_DONATE_URL_FORMAT = "https://payment.opay.tw/Broadcaster/CheckDonate/{0}";
    private static readonly Regex TOKEN_FETCH_REGEX = new Regex(
        @"\'<input name\=""__RequestVerificationToken"" type\=""hidden"" value\=""(.+)""");
    private readonly string _id;
    private readonly bool _shouldBeFakeSource;
    private readonly string _animationUrl;
    private readonly string _checkDonateUrl;

    private JsonObject[] _comments = Array.Empty<JsonObject>();
    public JsonObject[] Comments => _comments;
    
    private readonly HashSet<string> _existedDonateId = new HashSet<string>();

    public OpayFetcher(string id, bool shouldBeFakeSource)
    {
        _id = id;
        _shouldBeFakeSource = shouldBeFakeSource;
        _animationUrl = string.Format(
            OPAY_ANIMATION_URL_FORMAT, 
            id);
        _checkDonateUrl = string.Format(
            OPAY_CHECK_DONATE_URL_FORMAT, 
            id);
    }
    
    public async Task Fetch(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_id))
            return;

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
            "https://payment.opay.tw");
        
        var htmlResponse = await httpClient.GetAsync(
            _animationUrl, token);
        var htmlContent = await htmlResponse.Content.ReadAsStringAsync(token);
        var requestVerificationToken = TOKEN_FETCH_REGEX.Matches(htmlContent)[0]
            .Groups[1]
            .Value;

        while (!token.IsCancellationRequested)
        {
            try
            {
                var responseMessage = await httpClient.PostAsync(_checkDonateUrl, new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("__RequestVerificationToken", requestVerificationToken)
                }), token);
                
                var messages = JsonSerializer.Deserialize<OpayData>(
                    await responseMessage.Content.ReadAsStringAsync(token),
                    new JsonSerializerOptions
                    {
                        IncludeFields = true
                    })?.LstDonates ?? Array.Empty<OpayData.LstDonateData>();
                
                var newComments = messages
                    .Where(message => !_existedDonateId.Contains(message.DonateId))
                    .DistinctBy(message => message.DonateId)
                    .Select(message =>
                    {
                        _existedDonateId.Add(message.DonateId);

                        return new JsonObject(new[]
                        {
                            new KeyValuePair<string, JsonNode?>("id", $"opay"),
                            new KeyValuePair<string, JsonNode?>("service", _shouldBeFakeSource ? "youtube" : "opay"),
                            new KeyValuePair<string, JsonNode?>("name", "歐付寶訊息"),
                            new KeyValuePair<string, JsonNode?>("url", _checkDonateUrl),
                            new KeyValuePair<string, JsonNode?>("color", new JsonObject(new[]
                            {
                                new KeyValuePair<string, JsonNode?>("r", 56),
                                new KeyValuePair<string, JsonNode?>("g", 119),
                                new KeyValuePair<string, JsonNode?>("b", 0)
                            })),
                            new KeyValuePair<string, JsonNode?>("data", new JsonObject(new[]
                            {
                                new KeyValuePair<string, JsonNode?>("id", message.DonateId),
                                new KeyValuePair<string, JsonNode?>("liveId", _id),
                                new KeyValuePair<string, JsonNode?>("userId", message.Name),
                                new KeyValuePair<string, JsonNode?>("name", message.Name),
                                new KeyValuePair<string, JsonNode?>("profileImage", string.Empty),
                                new KeyValuePair<string, JsonNode?>("badges", new JsonArray()),
                                new KeyValuePair<string, JsonNode?>("isOwner", false),
                                new KeyValuePair<string, JsonNode?>("isModerator", false),
                                new KeyValuePair<string, JsonNode?>("isMember", false),
                                new KeyValuePair<string, JsonNode?>("autoModerated", false),
                                new KeyValuePair<string, JsonNode?>("hasGift", false),
                                new KeyValuePair<string, JsonNode?>("comment", message.Msg),
                                new KeyValuePair<string, JsonNode?>("timestamp",
                                    (double) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                                new KeyValuePair<string, JsonNode?>("paidText", $"${message.Amount}"),
                                new KeyValuePair<string, JsonNode?>("unit", "$"),
                                new KeyValuePair<string, JsonNode?>("price", message.Amount),
                                new KeyValuePair<string, JsonNode?>("colors", ColorUtility.GetColor(message.Amount)),
                                new KeyValuePair<string, JsonNode?>("displayName", message.Name),
                                new KeyValuePair<string, JsonNode?>("originalProfileImage", string.Empty),
                                new KeyValuePair<string, JsonNode?>("isFirstTime", true)
                            })),
                            new KeyValuePair<string, JsonNode?>("meta", new JsonObject(new[]
                            {
                                new KeyValuePair<string, JsonNode?>("interval", 0)
                            }))
                        });
                    });

                _comments = _comments.Concat(newComments).ToArray();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}