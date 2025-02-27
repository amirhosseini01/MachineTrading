using Hangfire;
using HtmlAgilityPack;
using MachineTrading.Enum;
using MachineTrading.Models;
using MachineTrading.Repository.Contracts;
using Microsoft.Playwright;
using Serilog.Core;

namespace MachineTrading.Services;

public class PlaywrightService(ISelectorRepo selectorRepo, IArticleRepo articleRepo, IAddressRepo addressRepo, ILogger<PlaywrightService> logger)
{
    private const string UserDataDir = @"C:\Users\Amir\Desktop\MachineTrading\_browserdata";
    const int ThirtyMinuteAsMilliSecond = 1_800_000;

    public async Task OpenBrowser(int addressId, CancellationToken ct = default)
    {
        var address = await addressRepo.FindAsync(addressId, ct);
        if (address is null)
        {
            throw new Exception("address not founded!");
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new BrowserTypeLaunchPersistentContextOptions { Headless = false, Timeout = ThirtyMinuteAsMilliSecond });

        var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();
        await page.GotoAsync(address.Url);
        await page.WaitForTimeoutAsync(100_000);
    }

    public async Task StartScrapping(int addressId, bool continueUntilPrevious = true, CancellationToken ct = default)
    {
        var address = await addressRepo.FindAsync(addressId, ct);
        if (address is null)
        {
            throw new Exception("address not founded!");
        }

        await StartScrapping(address, continueUntilPrevious, ct);
    }

    public async Task StartScrapping(Address address, bool continueUntilPrevious = true, CancellationToken ct = default)
    {
        
        var articles = new List<Article>();
        try
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new BrowserTypeLaunchPersistentContextOptions { Headless = false, Timeout =  ThirtyMinuteAsMilliSecond});

            var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();
            await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36" },
                { "Accept-Language", "en-US,en;q=0.9" },
                { "Referer", "https://www.google.com/" }
            });
            await page.AddInitScriptAsync(@"navigator.webdriver = undefined;");
            await page.EvaluateAsync("""
                                     () => {
                                                     Object.defineProperty(navigator, 'webdriver', { get: () => undefined });
                                                 }
                                     """);

            await page.GotoAsync(address.Url);
            await page.WaitForTimeoutAsync(2_000);
            var continueScrapping = true;
            
            var selectors = await selectorRepo.GetAll(ct: ct);
            var previousArticles = await articleRepo.GetAll(takeSize: 1000, ct: ct);
            var duplicateCounter = 0;
            while (continueScrapping)
            {
                duplicateCounter++;
                
                var articleSelector = selectors.First(x => x.Type == SelectorType.Article).Value;
                var allElementCount = await page.Locator(articleSelector)
                    .CountAsync();
                if (allElementCount == 0)
                {
                    continue;
                }

                var elements = await page.Locator(articleSelector)
                    .AllAsync();
                foreach (var element in elements)
                {
                    duplicateCounter++;
                    var elementCount = await element.CountAsync();
                    if (elementCount == 0)
                    {
                        logger.LogWarning($"no element founded! article count{articles.Count}, url: {address.Url}");
                        continue;
                    }

                    var link = await element
                        .Locator(selectors.First(x => x.Type == SelectorType.Link).Value)
                        .First
                        .GetAttributeAsync("href");

                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }

                    string? pinned = null;
                    var pinnedCount = await element.Locator(selectors.First(x => x.Type == SelectorType.Pinned).Value).CountAsync();
                    if (pinnedCount > 0)
                    {
                        pinned = await element.Locator(selectors.First(x => x.Type == SelectorType.Pinned).Value).First.InnerTextAsync();
                    }

                    if (articles.Any(x => x.Link == link)) continue;

                    if (previousArticles.Any(x => x.Link == link))
                    {
                        if (!string.IsNullOrEmpty(pinned) && pinned == nameof(SelectorType.Pinned))
                        {
                            continue;
                        }

                        if (continueUntilPrevious)
                        {
                            continueScrapping = false;
                            break;
                        }

                        // else
                        continue;
                    }

                    if (await articleRepo.IsExistLink(link: link, ct: ct)) continue;

                    duplicateCounter = 0;

                    var time = await element
                        .Locator(selectors.First(x => x.Type == SelectorType.Time).Value)
                        .First
                        .GetAttributeAsync("datetime");

                    string? text = null;
                    var textSelector = selectors.First(x => x.Type == SelectorType.Text).Value;
                    var textCount = await element
                        .Locator(textSelector)
                        .CountAsync();
                    if (textCount > 0)
                    {
                        text = await element
                            .Locator(textSelector)
                            .First
                            .InnerHTMLAsync();
                    }

                    var userTitle = await element
                        .Locator(selectors.First(x => x.Type == SelectorType.UserTitle).Value)
                        .First
                        .InnerTextAsync();

                    var commentCount = await element
                        .Locator(selectors.First(x => x.Type == SelectorType.Comment).Value)
                        .First
                        .InnerTextAsync();

                    var reShareCount = await element
                        .Locator(selectors.First(x => x.Type == SelectorType.ReShare).Value)
                        .First
                        .InnerTextAsync();

                    string? likeCountStr = null;
                    var likeSelector = selectors.First(x => x.Type == SelectorType.Like).Value;
                    var likeCount = await element
                        .Locator(likeSelector)
                        .CountAsync();
                    if (likeCount == 0)
                    {
                        var unlikeSelector = selectors.First(x => x.Type == SelectorType.UnLike).Value;
                        likeCount = await element
                            .Locator(unlikeSelector)
                            .CountAsync();
                        if (likeCount > 0)
                        {
                            likeCountStr = await element
                                .Locator(unlikeSelector)
                                .First
                                .InnerTextAsync();
                        }
                    }
                    else
                    {
                        likeCountStr = await element
                            .Locator(likeSelector)
                            .First
                            .InnerTextAsync();
                    }


                    string? parentLink = null;
                    if (articles.Count > 0)
                    {
                        var previousArticle = articles.Last();
                        if (userTitle != previousArticle.UserTitle)
                        {
                            parentLink = previousArticle.Link;
                        }
                    }

                    var article = new Article
                    {
                        AddressId = address.Id,
                        CommentCount = commentCount,
                        LikeCount = likeCountStr,
                        ReShareCount = reShareCount,
                        Text = CleanHtml(text),
                        Time = time!,
                        UserTitle = userTitle,
                        CreateDate = DateTime.UtcNow,
                        Link = link,
                        ParentLink = parentLink
                    };

                    articles.Add(article);
                }


                await page.EvaluateAsync("window.scrollBy(0, 300);");
                await page.WaitForTimeoutAsync(1_000);
                if (articles.Count > 100)
                {
                    await articleRepo.AddRangeAsync(articles, ct);
                    await articleRepo.SaveChangesAsync(ct);
                    articles = [];
                }
            }
            if (articles.Count > 0)
            {
                await articleRepo.AddRangeAsync(articles, ct);
                await articleRepo.SaveChangesAsync(ct);
                articles = [];
            }
        }
        catch (Exception e)
        {
            if (articles.Count > 0)
            {
                await articleRepo.AddRangeAsync(articles, ct);
                await articleRepo.SaveChangesAsync(ct);
            }
            logger.LogInformation($"{articles.Count} articles has been added to the database. by the {address.Url}");
            Console.WriteLine(e);
            throw;
        }
    }

    [DisableConcurrentExecution(timeoutInSeconds: 2_000)]
    public async Task StartScrapping(CancellationToken ct = default)
    {
        var addresses = await addressRepo.GetAll(ct);
        if (addresses.Count == 0)
        {
            return;
        }

        foreach (var address in addresses)
        {
            await StartScrapping(address: address, ct: ct);
        }
    }

    private static string? CleanHtml(string? html)
    {
        if (string.IsNullOrEmpty(html)) return null;

        // Load the HTML string into an HtmlDocument
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        // Select all nodes except <img> tags
        var nodesToRemove = htmlDoc.DocumentNode.SelectNodes("//*");

        if (nodesToRemove != null)
        {
            foreach (var node in nodesToRemove)
            {
                // Replace the node with its inner text (preserves text content)
                HtmlNode parent = node.ParentNode;
                parent.RemoveChild(node, true); // true to keep the inner text
            }
        }

        return htmlDoc.DocumentNode.InnerHtml;
    }
}