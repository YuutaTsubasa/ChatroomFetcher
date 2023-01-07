using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Writer;

public interface IWriter
{
    Task Write(JsonObject[] comments);
}