using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Fetcher;

public interface IFetcher
{
    JsonObject[] Comments { get; }

    Task Fetch(CancellationToken token);
}