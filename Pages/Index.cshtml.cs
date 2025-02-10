using MachineTrading.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MachineTrading.Pages;

public class IndexModel(PlaywrightService playwrightService) : PageModel
{
    public void OnGet()
    {
    }

    public async Task OnGetOpenBrowser(CancellationToken ct = default)
    {
        await playwrightService.OpenBrowser(ct);
    }

    public async Task OnGetStartScrapping(string url, bool continueUntilPrevious = true, CancellationToken ct = default)
    {
        await playwrightService.StartScrapping(url: url, continueUntilPrevious: continueUntilPrevious, ct: ct);
    }
}