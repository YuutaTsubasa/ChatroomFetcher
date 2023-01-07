using System.Text.Json.Nodes;

namespace ChatroomFetcherElectron.FetcherCore.Utility;

public static class ColorUtility
{
    public static JsonObject GetColor(int price)
    {
        if (price < 30)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(21,101,192,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(21,101,192,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(255,255,255,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(255,255,255,0.5019607843137255)")
            });
        
        if (price < 75)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(0,184,212,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(0,229,255,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(0,0,0,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(0,0,0,0.5019607843137255)")
            });
        
        if (price < 150)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(0,191,165,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(29,233,182,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(0,0,0,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(0,0,0,0.5019607843137255)")
            });
        
        if (price < 300)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(255,179,0,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(255,202,40,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(0,0,0,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(0,0,0,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(0,0,0,0.5019607843137255)")
            });
        
        if (price < 750)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(230,81,0,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(245,124,0,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(255,255,255,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(255,255,255,0.5019607843137255)")
            });
        
        if (price < 1500)
            return new JsonObject(new[]
            {
                new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(194,24,91,1)"),
                new KeyValuePair<string, JsonNode?>("headerTextColor",
                    "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(233,30,99,1)"),
                new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(255,255,255,0.8745098039215686)"),
                new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                    "rgba(255,255,255,0.5411764705882353)"),
                new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(255,255,255,0.5019607843137255)")
            });
        
        return new JsonObject(new[]
        {
            new KeyValuePair<string, JsonNode?>("headerBackgroundColor", "rgba(208,0,0,1)"),
            new KeyValuePair<string, JsonNode?>("headerTextColor",
                "rgba(255,255,255,0.8745098039215686)"),
            new KeyValuePair<string, JsonNode?>("bodyBackgroundColor", "rgba(230,33,23,1)"),
            new KeyValuePair<string, JsonNode?>("bodyTextColor", "rgba(255,255,255,0.8745098039215686)"),
            new KeyValuePair<string, JsonNode?>("authorNameTextColor",
                "rgba(255,255,255,0.5411764705882353)"),
            new KeyValuePair<string, JsonNode?>("timestampColor", "rgba(255,255,255,0.5019607843137255)")
        });
    }
}