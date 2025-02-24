using HtmlAgilityPack;
using MachineTrading.Data;
using MachineTrading.Enum;
using MachineTrading.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace MachineTrading.Pages;

public class IndexModel(MachineTradingContext context) : PageModel
{
    public const string UserDataDir = "C:\\Users\\Amir\\Desktop\\MachineTrading\\_browserdata";
    public void OnGet()
    {
    }

    public async Task OnGetOpenBrowser(CancellationToken ct = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new() { Headless = false, Timeout = 3_000_000 });

        var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();
        await page.WaitForTimeoutAsync(100_000);
    }

    public async Task OnGetStartScrapping(string url, CancellationToken ct = default)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchPersistentContextAsync(UserDataDir, new() { Headless = false, Timeout = 3_000_000 });

        var page = browser.Pages.Count > 0 ? browser.Pages[0] : await browser.NewPageAsync();

        await page.GotoAsync(url);
        await page.WaitForTimeoutAsync(2_000);
        var continueScrapping = true;
        var articles = new List<Article>();
        var selectors = await context.Selectors.OrderBy(x => x.Id).ToListAsync(ct);
        var previousArticles = await context.Articles.OrderByDescending(x=> x.Time).ToListAsync(ct);
        while (continueScrapping)
        {
            var articleSelector = selectors.First(x => x.Type == SelectorType.Article).Value;
            var allElementCount = await page.Locator(articleSelector)
                .CountAsync();
            if (allElementCount > 0)
            {
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

                    if (articles.Any(x => x.Link == link)) continue;
                    
                    if (previousArticles.Any(x => x.Link == link)) continue;

                    if (await context.Articles.AnyAsync(x => x.Link == link, ct)) continue;

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
                        Link = link!,
                        ParentLink = parentLink
                    };

                    articles.Add(article);
                }


                await page.EvaluateAsync("window.scrollBy(0, 500);");
                await page.WaitForTimeoutAsync(3_000);
            }
            else
            {
                continueScrapping = false;
            }

            if (articles.Count > 0)
            {
                await context.AddRangeAsync(articles, ct);
                await context.SaveChangesAsync(ct);
                articles = [];
            }
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