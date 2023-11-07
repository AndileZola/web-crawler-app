////using HtmlAgilityPack;
//using HtmlAgilityPack;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http;
//using System.Threading;
//using System.Xml;

//class WebCrawler
//{   
//    private string baseUrl;
//    //private string startUrl;
//    private HttpClient httpClient;
//    private List<string> urlsToCrawl; //List<string> extractedUrls = new List<string>();

//    private readonly List<string> crawledUrls;
//    private readonly object urlsToCrawlLock = new object(); // Lock object for synchronization
//    int crawledCount = 0;

//    //public WebCrawler(string startUrl)
//    //{
//    //    this.startUrl = startUrl;
//    //    this.baseUrl = new Uri(startUrl).Host;
//    //    this.httpClient = new HttpClient();
//    //    this.urlsToCrawl = new List<string>();
//    //    this.crawledUrls = new List<string>();
//    //}

//    public WebCrawler()
//    {
//        this.httpClient = new HttpClient();
//        this.urlsToCrawl = new List<string>();
//        this.crawledUrls = new List<string>();
//    }

//    public List<string> Crawl(string startUrl)
//    {
//        this.baseUrl = new Uri(startUrl).Host;
//        var tasks = new List<Task>();
//        var results = new List<string>();
//        AddToUrlsToCrawl(startUrl);    

//        while (true)
//        {
//            string urlToCrawl = GetNextUrlToCrawl();
//            if (urlToCrawl == null)
//                break; // No more URLs to crawl

//            if (!crawledUrls.Contains(urlToCrawl))
//            {
//                crawledUrls.Add(urlToCrawl);
//                tasks.Add(Task.Factory.StartNew(() =>
//                {
//                    CrawlPage(urlToCrawl, results);
//                }));
//            }
//        }

//        Task.WhenAll(tasks).Wait();
//        return results;
//    }

//    public static List<string> ExtractLinksInHtmlPage(string url, string htmlContent)
//    {
//        var extractedLinks = new List<string>();
//        var htmlDocument = new HtmlDocument(); //Using HtmlAgilityPack: https://html-agility-pack.net/traversing
//        htmlDocument.LoadHtml(htmlContent);

//        var links = htmlDocument.DocumentNode.SelectNodes("//a[@href]");

//        if (links != null && links.Count > 0)
//        {          
//            foreach (var link in links)
//            {
//                Uri absoluteUri;
//                var href = link.GetAttributeValue("href", "");
//                bool isValidUrl = Uri.TryCreate(new Uri(url), href, out absoluteUri);
//                if (isValidUrl)
//                {
//                    extractedLinks.Add(absoluteUri.AbsoluteUri);
//                }
//            }
//        }

//        return extractedLinks;
//    }


//    private void AddToUrlsToCrawl(string url)
//    {
//        lock (urlsToCrawlLock)
//        {
//            urlsToCrawl.Add(url);
//        }
//    }

//    private string GetNextUrlToCrawl()
//    {
//        lock (urlsToCrawlLock)
//        {
//            if (urlsToCrawl.Count > 0)
//            {
//                string url = urlsToCrawl[0];
//                urlsToCrawl.RemoveAt(0); //Do not crawl the same url twice
//                return url;
//            }
//            return null;
//        }
//    }

//    public void ParseHtml(string htmlContent)
//    {
//        var xmlDocument = new XmlDocument();
//        xmlDocument.LoadXml(htmlContent);

//        //ParseNodes(xmlDocument.ChildNodes);
//        if (xmlDocument.ChildNodes != null && xmlDocument.ChildNodes.Count > 0)
//        {
//            foreach (XmlNode node in xmlDocument.ChildNodes)
//            {
//                if (node is XmlElement element)
//                {
//                    if (element.Name == "a" && element.Attributes["href"] != null)
//                    {
//                        string href = element.Attributes["href"].Value;
//                        urlsToCrawl.Add(href);
//                    }
//                    // Recursively process child nodes
//                    ParseNodes(element.ChildNodes);
//                }
//            }
//        }
//    }

//    private static void ParseNodes(XmlNodeList nodes)
//    {
//        foreach (XmlNode node in nodes)
//        {
//            if (node is XmlElement element)
//            {
//                if (element.Name == "a" && element.Attributes["href"] != null)
//                {
//                    string href = element.Attributes["href"].Value;
//                    //urlsToCrawl.Add(href);
//                }              
//                // Recursively process child nodes
//                ParseNodes(element.ChildNodes);
//            }
//        }
//    }

//    private void CrawlPage2(string url, List<string> results)
//    {
//        try
//        {
//            var response = httpClient.GetAsync(url).Result;
//            var html = response.Content.ReadAsStringAsync().Result;

//            //Using HtmlAgilityPack library:  https://html-agility-pack.net/?z=codeplex
//            //var htmlDocument = new HtmlDocument();
//            //htmlDocument.LoadHtml(html); //Get serialized html page

//            //var allLinksInPage = htmlDocument.DocumentNode.SelectNodes("//a[@href]"); //Get all links in the serilized html page


//            using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
//            {
//                string htmlContent = client.DownloadString(url);
//                var extractedLinks = ExtractLinksInHtmlPage(url,htmlContent);
//            }

//            //ExtractLinksInHtmlPage(html);


//            //if (urlsToCrawl != null && urlsToCrawl.Count > 0)
//            //{
//            //    foreach (var link in urlsToCrawl)
//            //    {
//            //        var href = link.GetAttributeValue("href", "");
//            //        if (!string.IsNullOrWhiteSpace(href))
//            //        {
//            //            Uri uri = new Uri(url);
//            //            if (Uri.TryCreate(new Uri(url), href, out uri) && uri.Host == baseUrl)
//            //            {
//            //                urlsToCrawl.Add(uri.AbsoluteUri);
//            //                results.Add(uri.AbsoluteUri);
//            //            }
//            //        }
//            //    }
//            //}
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Error occured while crawling the page: {ex.Message}");
//        }
//    }

//    private List<string> CrawlPage(string url, List<string> results)
//    {
//        try
//        {
//            //var response = httpClient.GetAsync(url).Result;
//            //var html = response.Content.ReadAsStringAsync().Result;

//            using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
//            {
//                string htmlContent = client.DownloadString(url);
//                //var extractedLinks = ExtractLinksInHtmlPage(url, htmlContent);

//                var document = new HtmlDocument();
//                document.LoadHtml(htmlContent);

//                var links = document.DocumentNode.SelectNodes("//a[@href]").Skip(1);
//                if (links != null)
//                {
//                    foreach (var link in links)
//                    {
//                        var href = link.GetAttributeValue("href", "");
//                        if (!string.IsNullOrWhiteSpace(href))
//                        {
//                            //if (crawledUrls.Contains(url))
//                            //    break;

//                            Uri absoluteUri;
//                            if (Uri.TryCreate(new Uri(url), href, out absoluteUri) && absoluteUri.Host == baseUrl) //only links that are under the same hostname as startUrl.
//                            {
//                                //urlsToCrawl.Add(absoluteUri.AbsoluteUri);
//                                //results.Add(absoluteUri.AbsoluteUri);
//                                crawledUrls.Add(absoluteUri.AbsoluteUri);
//                                //var all = CrawlPage(absoluteUri.AbsoluteUri, results);
//                                Crawl(absoluteUri.AbsoluteUri);
//                                //results.AddRange(all);
//                                //Crawl only once
//                                urlsToCrawl.Remove(absoluteUri.AbsoluteUri);
//                                crawledCount = results.Count();
//                            }
//                        }
//                    }
//                }
//            }
//            return results;
//        }
//        catch (Exception ex)
//        {
//            throw new Exception($"Error occured while crawling the page: {ex.Message}");
//        }
//        return null;
//    }
//}

//class Program2
//{
//    static void Main(string[] args)
//    {
//        string startUrl = "https://en.wikipedia.org/wiki/South_Africa"; // Replace with your starting URL
//        var crawler = new WebCrawler();
//        var crawledUrls = crawler.Crawl(startUrl);

//        foreach (var url in crawledUrls)
//        {
//            Console.WriteLine(url);
//        }
//    }
//}
