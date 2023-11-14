using Microsoft.AspNetCore.Mvc;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.RegularExpressions;
using System.Globalization;
using OpenQA.Selenium.Support.UI;

namespace NewsScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NewsController : ControllerBase
    {
        [Route("NewsList")]
        [AcceptVerbs("GET")]
        public async Task<Output> GetNewsList(string? languageCode)
        {
            Output output = new Output();
            try
            {
                List<NewsWebsite> sList = new List<NewsWebsite>();
                List<News> list = new List<News>();
                List<News> saysList = new List<News>();
                List<News> utusanList = new List<News>();
                List<News> bhList = new List<News>();

                #region Website Declaration
                var says = new NewsWebsite()
                {
                    WebsiteName = "Says",
                    WebsiteURL = "https://says.com/my/news"
                };
                sList.Add(says);

                var utusan = new NewsWebsite()
                {
                    WebsiteName = "Utusan",
                    WebsiteURL = "https://www.utusan.com.my/terkini/"
                };
                sList.Add(utusan);

                var bharian = new NewsWebsite()
                {
                    WebsiteName = "Berita Harian",
                    WebsiteURL = "https://www.bharian.com.my/berita"
                };
                sList.Add(bharian);
                #endregion

                var options = new ChromeOptions();
                options.AddArguments("headless");
                options.AddArguments("--window-size=1920,1080");
                options.AddArguments("--ignore-certificate-errors");
                options.AddArguments("--allow-running-insecure-content");
                options.AddArguments("--disable-web-security");
                options.AddArguments("--user-agent=Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3112.50 Safari/537.36");
                IWebDriver? driver = new ChromeDriver(options);

                if (driver != null)
                {
                    foreach (var i in sList)
                    {
                        driver?.Navigate().GoToUrl(i.WebsiteURL);
                        if (i.WebsiteName == "Says" && (languageCode == "EN" || languageCode == null))
                        {
                            var waitForElementSays = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("news-feed-items")));
                            if (!string.IsNullOrWhiteSpace(waitForElementSays.GetAttribute("innerText")))
                            {
                                IWebElement? saysNewsFeedDiv = driver?.FindElement(By.ClassName("news-feed-items"));
                                IReadOnlyCollection<IWebElement>? saysNewsFeed = saysNewsFeedDiv?.FindElements(By.TagName("li"));

                                if (saysNewsFeed != null && saysNewsFeed.Count > 0)
                                {
                                    foreach (var saysNews in saysNewsFeed)
                                    {
                                        var title = saysNews.FindElement(By.ClassName("story-info")).FindElement(By.TagName("h3")).FindElement(By.ClassName("ga-channel-story")).FindElement(By.TagName("p")).GetAttribute("innerText");
                                        var imageUrl = saysNews.FindElement(By.ClassName("story-cover-image")).FindElement(By.ClassName("story-image")).GetAttribute("data-lazy") == null ? "https://archive.org/download/placeholder-image/placeholder-image.jpg" : saysNews.FindElement(By.ClassName("story-cover-image")).FindElement(By.ClassName("story-image")).GetAttribute("data-lazy");
                                        var oriNewsUrl = saysNews.FindElement(By.ClassName("story-info")).FindElement(By.TagName("h3")).FindElement(By.ClassName("ga-channel-story")).GetAttribute("href") + "/";

                                        var saysNewsList = new News()
                                        {
                                            Title = title,
                                            ImageURL = imageUrl,
                                            OriNewsURL = oriNewsUrl,
                                            Publisher = i.WebsiteName
                                        };
                                        saysList.Add(saysNewsList);
                                        if (saysList.Count == 10)
                                        {
                                            break;
                                        }
                                    }

                                    IWebDriver? publishedDateDriver = new ChromeDriver(options);
                                    if (publishedDateDriver != null)
                                    {
                                        foreach (var sL in saysList)
                                        {
                                            publishedDateDriver?.Navigate().GoToUrl(sL.OriNewsURL);
                                            var waitForElementPD = new WebDriverWait(publishedDateDriver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("story-meta")));
                                            if (!string.IsNullOrWhiteSpace(waitForElementPD.GetAttribute("innerText")))
                                            {
                                                IWebElement? publishedDateDiv = publishedDateDriver?.FindElement(By.ClassName("story-meta"));
                                                var publishedDateNode = publishedDateDiv?.FindElement(By.TagName("p")).GetAttribute("innerText");
                                                if (!string.IsNullOrWhiteSpace(publishedDateNode))
                                                {
                                                    var publishedDate = Regex.Match(publishedDateNode, "(0?[1-9]|[12][0-9]|3[01])\\s+[A-Za-z]+\\s+[0-9]+,\\s+(0?[0-9]|1[0-9]|2[0-3]):[0-9]+\\s+[A-Za-z]+");
                                                    sL.PublishedDate = DateTime.Parse(publishedDate.Value);
                                                }
                                            }
                                        }
                                        publishedDateDriver.Quit();
                                    }
                                    list.AddRange(saysList);
                                }
                            }
                        }

                        if (i.WebsiteName == "Utusan" && (languageCode == "BM" || languageCode == null))
                        {
                            var waitForElementUtusan = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.TagName("article")));
                            if (!string.IsNullOrWhiteSpace(waitForElementUtusan.GetAttribute("innerText")))
                            {
                                IReadOnlyCollection<IWebElement>? utusanNewsFeed = driver?.FindElements(By.TagName("article"));

                                if (utusanNewsFeed != null)
                                {
                                    foreach (var utusanNews in utusanNewsFeed)
                                    {
                                        var title = utusanNews.FindElement(By.ClassName("jeg_postblock_content")).FindElement(By.ClassName("jeg_post_title")).FindElement(By.TagName("a")).GetAttribute("innerText");
                                        var imageUrl = utusanNews.FindElement(By.ClassName("jeg_thumb")).FindElement(By.ClassName("thumbnail-container")).FindElement(By.TagName("img")).GetAttribute("src") == null ? "https://archive.org/download/placeholder-image/placeholder-image.jpg" : utusanNews.FindElement(By.ClassName("jeg_thumb")).FindElement(By.ClassName("thumbnail-container")).FindElement(By.TagName("img")).GetAttribute("src");
                                        var oriNewsUrl = utusanNews.FindElement(By.ClassName("jeg_thumb")).FindElement(By.TagName("a")).GetAttribute("href");
                                        var publishedDateNode = utusanNews.FindElement(By.ClassName("jeg_postblock_content")).FindElement(By.ClassName("jeg_post_meta")).FindElement(By.ClassName("jeg_meta_date")).FindElement(By.TagName("a")).GetAttribute("innerText");
                                        var PublishedDate = DateTime.Parse(publishedDateNode);

                                        var utusanNewsList = new News()
                                        {
                                            Title = title,
                                            ImageURL = imageUrl,
                                            OriNewsURL = oriNewsUrl,
                                            Publisher = i.WebsiteName,
                                            PublishedDate = PublishedDate,
                                        };
                                        utusanList.Add(utusanNewsList);
                                        if (utusanList.Count == 10)
                                        {
                                            break;
                                        }
                                    }
                                    list.AddRange(utusanList);
                                }
                            }
                        }

                        if (i.WebsiteName == "Berita Harian" && (languageCode == "BM" || languageCode == null))
                        {
                            var waitForElementBH = new WebDriverWait(driver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("latest-listing")));
                            if (!string.IsNullOrWhiteSpace(waitForElementBH.GetAttribute("innerText")))
                            {
                                IWebElement? bhNewsFeedDiv = driver?.FindElement(By.ClassName("latest-listing"));
                                IReadOnlyCollection<IWebElement>? bhNewsFeed = bhNewsFeedDiv?.FindElements(By.ClassName("article"));

                                if (bhNewsFeed != null && bhNewsFeed.Count() > 0)
                                {
                                    foreach (var bhNews in bhNewsFeed)
                                    {
                                        var title = bhNews.FindElement(By.ClassName("content")).FindElement(By.ClassName("field-title")).GetAttribute("innerText");
                                        var imageUrl = bhNews.FindElement(By.ClassName("field-image-th")).FindElement(By.TagName("img")).GetAttribute("src") == null ? "https://archive.org/download/placeholder-image/placeholder-image.jpg" : bhNews.FindElement(By.ClassName("field-image-th")).FindElement(By.TagName("img")).GetAttribute("src");
                                        var oriNewsUrl = bhNews.GetAttribute("href");

                                        var bhNewsList = new News()
                                        {
                                            Title = title,
                                            ImageURL = imageUrl,
                                            OriNewsURL = oriNewsUrl,
                                            Publisher = i.WebsiteName
                                        };
                                        bhList.Add(bhNewsList);
                                        if (bhList.Count == 10)
                                        {
                                            break;
                                        }
                                    }

                                    IWebDriver? publishedDateDriver = new ChromeDriver(options);
                                    if (publishedDateDriver != null)
                                    {
                                        foreach (var bhL in bhList)
                                        {
                                            publishedDateDriver?.Navigate().GoToUrl(bhL.OriNewsURL);
                                            var waitForElementPD = new WebDriverWait(publishedDateDriver, TimeSpan.FromSeconds(30)).Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.ClassName("article-meta")));
                                            if (!string.IsNullOrWhiteSpace(waitForElementPD.GetAttribute("innerText")))
                                            {
                                                IWebElement? publishedDateDiv = publishedDateDriver?.FindElement(By.ClassName("article-meta"));
                                                var publishedDateNode = publishedDateDiv?.FindElement(By.TagName("div")).GetAttribute("innerText");
                                                if (!string.IsNullOrWhiteSpace(publishedDateNode))
                                                {
                                                    var publishedDate = Regex.Match(publishedDateNode, "[A-Za-z]+\\s+(0?[1-9]|[12][0-9]|3[01]),\\s+[0-9]+\\s+@\\s+(0?[0-9]|1[0-9]|2[0-3]):[0-9]+[A-Za-z]+");
                                                    if (publishedDate.Value.Contains("am"))
                                                    {
                                                        bhL.PublishedDate = DateTime.ParseExact(publishedDate.Value, "MMMM d, yyyy @ h:mm'am'", CultureInfo.InvariantCulture);
                                                    }
                                                    else
                                                    {
                                                        bhL.PublishedDate = DateTime.ParseExact(publishedDate.Value, "MMMM d, yyyy @ h:mm'pm'", CultureInfo.InvariantCulture);
                                                    }
                                                }
                                            }

                                        }
                                        publishedDateDriver.Quit();
                                    }
                                    list.AddRange(bhList);
                                }
                            }

                        }
                    }
                    driver.Quit();
                    list  = list.OrderByDescending(a => a.PublishedDate).ToList();
                    output = new Output()
                    {
                        News = list,
                        Publishers = sList,
                    };
                }
            }
            catch (Exception ex)
            {
                output = new Output()
                {
                    Meta = new Meta()
                    {
                        ErrorMessage = ex.Message,
                        ErrorType = ex.GetType().Name
                    }
                };
            }
            finally
            {
                Response.OnCompleted(async () =>
                {
                    await Task.Delay(TimeSpan.FromHours(1)).ContinueWith(async t =>
                    {
                        await GetNewsList(languageCode);
                    });
                });
            }
            return output;
        }
    }
}

public class News
{
    public string? Title { get; set; }
    public string? ImageURL { get; set; }
    public DateTime? PublishedDate { get; set; }
    public string? Publisher { get; set; }
    public string? OriNewsURL { get; set; }
}
public class NewsWebsite
{
    public string? WebsiteName { get; set; }
    public string? WebsiteURL { get; set; }
}

public class Meta
{
    public string? ErrorType { get; set; }
    public string? ErrorMessage { get; set; }
}

public class Output
{
    public List<News>? News { get; set; }
    public List<NewsWebsite>? Publishers { get; set; }
    public Meta? Meta { get; set; }
}