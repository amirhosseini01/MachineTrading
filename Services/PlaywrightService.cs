using HtmlAgilityPack;
using MachineTrading.Enum;
using MachineTrading.Models;
using MachineTrading.Repository.Contracts;
using Microsoft.Playwright;

namespace MachineTrading.Services;

public class PlaywrightService(ISelectorRepo selectorRepo, IArticleRepo articleRepo, IAddressRepo addressRepo)
{
    private const string UserDataDir = @"C:\Users\Amir\Desktop\MachineTrading\_browserdata";

    public async Task OpenBrowser(int addressId, CancellationToken ct = default)
    {
        var address = await addressRepo.FindAsync(addressId, ct);
        if (address is null)
        {
            throw new Exception("address not founded!");
        }
        
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new BrowserTypeLaunchPersistentContextOptions { Headless = false, Timeout = 3_000_000 });

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

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new BrowserTypeLaunchPersistentContextOptions { Headless = false, Timeout = 3_000_000 });

        var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();

        await page.GotoAsync(address.Url);
        await page.WaitForTimeoutAsync(2_000);
        var continueScrapping = true;
        var articles = new List<Article>();
        var selectors = await selectorRepo.GetAll(ct: ct);
        var previousArticles = await articleRepo.GetAll(takeSize: 1000, ct: ct);
        while (continueScrapping)
        {
            var articleSelector = selectors.First(x => x.Type == SelectorType.Article).Value;
            var allElementCount = await page.Locator(articleSelector)
                .CountAsync();
            if (allElementCount == 0)
            {
                return;
            }


            var elements = await page.Locator(articleSelector)
                .AllAsync();
            foreach (var element in elements)
            {
                var elementCount = await element.CountAsync();
                if (elementCount == 0) continue;

                var link = await element
                    .Locator(selectors.First(x => x.Type == SelectorType.Link).Value)
                    .First
                    .GetAttributeAsync("href");

                if (string.IsNullOrEmpty(link))
                {
                    return;
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


            await page.EvaluateAsync("window.scrollBy(0, 500);");
            await page.WaitForTimeoutAsync(3_000);
        }

        if (articles.Count > 0)
        {
            await articleRepo.AddRangeAsync(articles, ct);
            await articleRepo.SaveChangesAsync(ct);
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