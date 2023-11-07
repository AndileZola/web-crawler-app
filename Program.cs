using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

class Program
{
    static async Task Main(string[] args)
    {
        //Log all crawled urls
        //string logFilePath  =  $"C:\\Crawler_Logs\\crawl_logging_{DateTime.Now.ToString("ddmmyyyyhhmm")}.txt";
        //if (!Directory.Exists(logFilePath))
        //    Directory.CreateDirectory(logFilePath);
        string logFilePath = $"crawl_logging_{DateTime.Now.ToString("ddmmyyyyhhmm")}.txt";
        TextWriterTraceListener textListener = new TextWriterTraceListener(logFilePath);
        Trace.Listeners.Add(textListener);
        Trace.AutoFlush = true;
        Console.WriteLine("Start Time:{0}", DateTime.Now);
        Stopwatch sw = new Stopwatch();
        sw.Start();
        Console.WriteLine("Please enter startUrl: ");
        string startUrl = Console.ReadLine(); // Read user input
        Console.WriteLine($"StartUrl: {startUrl}");
        //string startUrl = "https://mhlontlolm.gov.za/"; //"https://en.wikipedia.org/wiki/South_Africa"; // Replace with your starting URL
        var crawler = new WebCrawler(startUrl);

        //Start Crawling. This might take a while
        await crawler.CrawlAsync();
        sw.Stop();
        Console.WriteLine("Elapsed={0}", sw.Elapsed);
        Console.WriteLine("Stop Time:{0}", DateTime.Now);
        Trace.Close();
        textListener.Close();
        Console.WriteLine("Crawling Complete!!!!!");
        Console.WriteLine("Log file: " + logFilePath);
        Console.ReadKey();
    }
}
class WebCrawler
{
    private HttpClient httpClient;
    private string baseUrl;
    private ConcurrentQueue<string> urlsToCrawl;
    private HashSet<string> crawledUrls;

    public WebCrawler(string startUrl)
    {
        httpClient = new HttpClient();
        baseUrl = new Uri(startUrl).Host;
        urlsToCrawl = new ConcurrentQueue<string>();
        crawledUrls = new HashSet<string>();

        urlsToCrawl.Enqueue(startUrl);
    }

    public async Task CrawlAsync()
    {
        //Avoid crawling the same url twice.
        while (urlsToCrawl.TryDequeue(out string url)) 
        {
            //Url already crawed. Don't crawl again
            if (crawledUrls.Contains(url))
                continue; // URL already crawled

            Console.WriteLine("Crawled: {0}", url);
            Trace.WriteLine("Crawled: " + url);

            //Get the html code fo the page into a string
            var pageContent = await DownloadPageAsync(url);
            if (pageContent == null)
                continue; // Error while downloading

            //Get all links found in that page
            var links = ExtractLinksInHtmlPage(url, pageContent);

            foreach (var link in links)
            {
                //All all links to the list of URL's yet to crawl
                urlsToCrawl.Enqueue(link);
            }

            //collection of only links found in thd downloaded page
            crawledUrls.Add(url);
        }
    }

    private async Task<string> DownloadPageAsync(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while downloading the page: {ex.Message}");
            return null;
        }
    }

    public static List<string> ExtractLinksInHtmlPage(string url, string htmlContent)
    {
        var extractedLinks = new List<string>();
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var links = htmlDocument.DocumentNode.SelectNodes("//a[@href]");

        if (links != null && links.Count > 0)
        {
            foreach (var link in links)
            {
                Uri absoluteUri;
                var href = link.GetAttributeValue("href", "");
                if (Uri.TryCreate(new Uri(url), href, out absoluteUri) && absoluteUri.Host == new Uri(url).Host)
                {
                    extractedLinks.Add(absoluteUri.AbsoluteUri);
                }
            }
        }

        return extractedLinks;
    }

    private async Task CrawlPageAsync(string url, List<string> results)
    {
        try
        {
            if (crawledUrls.Contains(url))
            {
                return; // this URL is already crawled
            }

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync();

            var document = new HtmlDocument();
            document.LoadHtml(htmlContent);

            var links = document.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", "");
                    if (!string.IsNullOrWhiteSpace(href))
                    {
                        Uri absoluteUri;
                        if (Uri.TryCreate(new Uri(url), href, out absoluteUri) && absoluteUri.Host == baseUrl)
                        {
                            urlsToCrawl.Enqueue(absoluteUri.AbsoluteUri);
                            results.Add(absoluteUri.AbsoluteUri);
                        }
                    }
                }
            }

            crawledUrls.Add(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred while crawling the page: {ex.Message}");
        }
    }
}

