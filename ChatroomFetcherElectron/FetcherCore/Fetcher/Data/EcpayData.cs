using System.Text.Json.Serialization;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher.Data;

[Serializable]
public class EcpayData
{
    [JsonPropertyName("donateid")] public string DonateId;
    [JsonPropertyName("name")] public string Name;
    [JsonPropertyName("msg")] public string Msg;
    [JsonPropertyName("amount")] public int Amount;
}