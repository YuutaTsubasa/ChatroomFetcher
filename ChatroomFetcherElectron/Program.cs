using System.Text.Json;
using ChatroomFetcherElectron.FetcherCore.Fetcher;
using ChatroomFetcherElectron.FetcherCore.Writer;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Newtonsoft.Json.Linq;

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
        Electron.IpcMain.On("connectToFetch", async (args) =>
        {
            try
            {
                var argList = (JArray) args;
                var ecpayId = (string) argList[0];
                var ecpayFakeSource = (bool) argList[1];
                var opayId = (string) argList[2];
                var opayFakeSource = (bool) argList[3];
                var oneCommeUrl = (string) argList[4];
                var oneCommeTemplatePath = (string) argList[5];

                var fetchers = new IFetcher[]
                {
                    new EcpayFetcher(ecpayId, ecpayFakeSource),
                    new OpayFetcher(opayId, opayFakeSource),
                    new OneCommeFetcher(oneCommeUrl)
                };
                
                var writers = new IWriter[]
                {
                    new OneCommeWriter(oneCommeTemplatePath)
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