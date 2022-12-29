using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ChatroomFetcher.YoutubeData;
using Microsoft.Web.WebView2.Wpf;

namespace ChatroomFetcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private async void FetchButton_OnClick(object sender, RoutedEventArgs e)
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
            
            var chatroomHtmlContent = await httpClient.GetStringAsync("https://www.youtube.com/live_chat?is_popout=1&v=Lec6vHJVue4");

            var key = (new Regex("\"INNERTUBE_API_KEY\":\"(.+)\",\"INNERTUBE_API_VERSION\""))
                .Matches(chatroomHtmlContent)[0]
                .Groups[1]
                .Value;

            var url = $"https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?key={key}&prettyPrint=false";
            var body = new YoutubeChatRequestData
            {
                Context = new YoutubeChatRequestData.ContextData(),
                Continuation = "",
                WebClientInfo = new YoutubeChatRequestData.WebClientInfoData()
            };
            var result = await httpClient.PostAsJsonAsync(url, body);

            ResultTextBlock.Text = url + "\n" + await result.Content.ReadAsStringAsync();
        }
    }
}