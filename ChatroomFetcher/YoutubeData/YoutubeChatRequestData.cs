using System;
using System.Text.Json.Serialization;

namespace ChatroomFetcher.YoutubeData;

[Serializable]
public class YoutubeChatRequestData
{
    [Serializable]
    public class UserData
    {
        [JsonPropertyName("lockedSafetyMode")] public bool LockedSafetyMode;
    }
    
    [Serializable]
    public class RequestData
    {
        [JsonPropertyName("consistencyTokenJars")] public string[] ConsistencyTokenJars;
        [JsonPropertyName("internalExperimentFlags")] public string[] InternalExperimentFlags;
        [JsonPropertyName("useSsl")] public bool UseSsl;
    }
    
    [Serializable]
    public class MainAppWebInfoData
    {
        [JsonPropertyName("graftUrl")] public string GraftUrl;
        [JsonPropertyName("isWebNativeShareAvailable")] public bool IsWebNativeShareAvailable;
        [JsonPropertyName("webDisplayMode")] public string WebDisplayMode;
    }
    
    [Serializable]
    public class ConfigInfoData
    {
        [JsonPropertyName("appInstallData")] public string AppInstallData;
    }
    
    [Serializable]
    public class ClickTrackingData
    {
        [JsonPropertyName("clickTrackingParams")] public string Params;
    }

    [Serializable]
    public class ClientData
    {
        [JsonPropertyName("acceptHeader")] public string AcceptHeader;
        [JsonPropertyName("browserName")] public string BrowserName;
        [JsonPropertyName("browserVersion")] public string BrowserVersion;
        [JsonPropertyName("clientFormFactor")] public string ClientFormFactor;
        [JsonPropertyName("clientName")] public string ClientName;
        [JsonPropertyName("clientVersion")] public string ClientVersion;
        [JsonPropertyName("configInfo")] public ConfigInfoData ConfigInfo;
        [JsonPropertyName("connectionType")] public string ConnectionType;
        [JsonPropertyName("deviceExperimentId")] public string DeviceExperimentId;
        [JsonPropertyName("deviceMake")] public string DeviceMake;
        [JsonPropertyName("deviceModel")] public string DeviceModel;
        [JsonPropertyName("gl")] public string Gl;
        [JsonPropertyName("hl")] public string Hl;
        [JsonPropertyName("mainAppWebInfo")] public MainAppWebInfoData MainAppWebInfo;
        [JsonPropertyName("memoryTotalKbytes")] public string MemoryTotalKbytes;
        [JsonPropertyName("originalUrl")] public string OriginalUrl;
        [JsonPropertyName("osName")] public string OsName;
        [JsonPropertyName("osVersion")] public string OsVersion;
        [JsonPropertyName("platform")] public string Platform;
        [JsonPropertyName("remoteHost")] public string RemoteHost;
        [JsonPropertyName("screenDensityFloat")] public float ScreenDensityFloat;
        [JsonPropertyName("screenHeightPoints")] public int ScreenHeightPoints; 
        [JsonPropertyName("screenPixelDensity")] public int ScreenPixelDensity;
        [JsonPropertyName("screenWidthPoints")] public int ScreenWidthPoints;
        [JsonPropertyName("timeZone")] public string TimeZone;
        [JsonPropertyName("userAgent")] public string UserAgent;
        [JsonPropertyName("userInterfaceTheme")] public string UserInterfaceTheme;
        [JsonPropertyName("utcOffsetMinutes")] public int UtcOffsetMinutes;
        [JsonPropertyName("visitorData")] public string VisitorData;
        [JsonPropertyName("request")] public RequestData Request;
        [JsonPropertyName("user")] public UserData User;
    }
    
    [Serializable]
    public class ContextData
    {
        [JsonPropertyName("clickTracking")] public ClickTrackingData ClickTracking;
    }

    [Serializable]
    public class WebClientInfoData
    {
        [JsonPropertyName("isDocumentHidden")] public bool IsDocumentHidden;
    }
    
    [JsonPropertyName("context")] public ContextData Context;
    [JsonPropertyName("continuation")] public string Continuation;
    [JsonPropertyName("webClientInfo")] public WebClientInfoData WebClientInfo;
}