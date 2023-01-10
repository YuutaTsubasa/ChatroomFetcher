using System.Text.Json;
using ChatroomFetcherElectron.FetcherCore.Fetcher;
using ChatroomFetcherElectron.FetcherCore.Writer;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Newtonsoft.Json.Linq;

const string CONFIG_FILE_NAME = "config.json";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.WebHost.UseElectron(args);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


Task.Run(async () =>
{
    var browserWindow = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
    {
        Width = 800,
        Height = 600,
        Show = false
    });

    browserWindow.OnReadyToShow += () =>
    {
        Electron.IpcMain.On("saveConfig", (args) =>
        {
            File.WriteAllText(CONFIG_FILE_NAME, (string)args);
        });
        
        Electron.IpcMain.On("connectToFetch", async (args) =>
        {
            try
            {
                var argList = (JArray) args;
                var youtubeLiveId = (string) argList[0];
                var ecpayId = (string) argList[1];
                var ecpayFakeSource = (bool) argList[2];
                var opayId = (string) argList[3];
                var opayFakeSource = (bool) argList[4];
                var oneCommeUrl = (string) argList[5];
                var oneCommeTemplatePath = (string) argList[6];

                var fetchers = new IFetcher[]
                {
                    new YoutubeFetcher(youtubeLiveId),
                    new EcpayFetcher(ecpayId, ecpayFakeSource),
                    new OpayFetcher(opayId, opayFakeSource),
                    new OneCommeFetcher(oneCommeUrl)
                };
                
                var writers = new IWriter[]
                {
                    new OneCommeWriter(oneCommeTemplatePath),
                    new NewCommentActionWriter(newComments =>
                    {
                        Electron.IpcMain.Send(browserWindow, "message", JsonSerializer.Serialize(newComments));
                    })
                };
                var cancellationTokenSource = new CancellationTokenSource();
                await Task.WhenAll(fetchers.Select(fetcher => fetcher.Fetch(cancellationTokenSource.Token))
                    .Append(WriteAllComments(fetchers, writers, cancellationTokenSource.Token)));
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.StackTrace);
            }
            
        });

        if (File.Exists(CONFIG_FILE_NAME))
        {
            var configString = File.ReadAllText(CONFIG_FILE_NAME);
            Electron.IpcMain.Send(browserWindow, "loadConfig", configString);
        }
        
        browserWindow.Show();
    };
});

app.Run();

async Task WriteAllComments(
    IFetcher[] fetchers,
    IWriter[] writers, 
    CancellationToken token)
{
    try
    {
        var existedIds = new HashSet<string>();
        while (!token.IsCancellationRequested)
        {
            var comments = fetchers.SelectMany(fetcher => fetcher.Comments)
                .OrderBy(comment => comment["data"]["timestamp"].GetValue<double>())
                .ToArray();
            
            var allCommentIds = comments.Select(comment => comment["data"]["id"].GetValue<string>()).ToArray();
            var ids = existedIds;
            if (allCommentIds.Any(id => !ids.Contains(id)))
            {
                foreach (var writer in writers)
                {
                    await writer.Write(comments);
                }

                existedIds = new HashSet<string>(allCommentIds);
            }

            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception.Message);
        Console.WriteLine(exception.StackTrace);
    }
}