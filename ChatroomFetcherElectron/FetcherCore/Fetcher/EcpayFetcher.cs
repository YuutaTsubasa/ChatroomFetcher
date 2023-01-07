using System.Text.Json;
using System.Text.Json.Nodes;
using ChatroomFetcherElectron.FetcherCore.Fetcher.Data;
using ChatroomFetcherElectron.FetcherCore.Utility;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public class EcpayFetcher : IFetcher
{
    private const string ECPAY_CHECK_DONATE_URL_FORMAT = "https://payment.ecpay.com.tw/Broadcaster/CheckDonate/{0}";
    private readonly string _id;
    private readonly bool _shouldBeFakeSource;
    private readonly string _checkDonateUrl;

    private JsonObject[] _comments = Array.Empty<JsonObject>();
    public JsonObject[] Comments => _comments;
    
    private readonly HashSet<string> _existedDonateId = new HashSet<string>();

    public EcpayFetcher(string id, bool shouldBeFakeSource)
    {
        _id = id;
        _shouldBeFakeSource = shouldBeFakeSource;
        _checkDonateUrl = string.Format(
            ECPAY_CHECK_DONATE_URL_FORMAT, 
            id);
    }
    
    public async Task Fetch(CancellationToken token)
    {
        if (string.IsNullOrEmpty(_id))
            return;

        var httpClient = new HttpClient();
        while (!token.IsCancellationRequested)
        {
            var responseMessage = await httpClient.PostAsync(
                _checkDonateUrl, null, token);
            var messages = JsonSerializer.Deserialize<EcpayData[]>(
                await responseMessage.Content.ReadAsStringAsync(token), 
                new JsonSerializerOptions
                {
                    IncludeFields = true
                });

            var newComments = messages
                .Where(message => !_existedDonateId.Contains(message.DonateId))
                .DistinctBy(message => message.DonateId)
                .Select(message =>
                {
                    _existedDonateId.Add(message.DonateId);
                    
                    return new JsonObject(new[]
                    {
                        new KeyValuePair<string, JsonNode?>("id", $"ecpay"),
                        new KeyValuePair<string, JsonNode?>("service", _shouldBeFakeSource ? "youtube" : "ecpay"),
                        new KeyValuePair<string, JsonNode?>("name", "綠界訊息"),
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
                            new KeyValuePair<string, JsonNode?>("timestamp", (double)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
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
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}