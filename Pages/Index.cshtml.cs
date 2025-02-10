using MachineTrading.Models;
using MachineTrading.Repository.Contracts;
using MachineTrading.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MachineTrading.Pages;

public class IndexModel(PlaywrightService playwrightService, IAddressRepo addressRepo) : PageModel
{
    public List<Address>? Addresses { get; set; }
    public async Task OnGet(CancellationToken ct = default)
    {
        Addresses = await addressRepo.GetAll(ct);
    }

    public async Task OnGetOpenBrowser(int addressId, CancellationToken ct = default)
    {
        await playwrightService.OpenBrowser(addressId: addressId, ct);
    }

    public async Task OnGetStartScrapping(int addressId, bool continueUntilPrevious = true, CancellationToken ct = default)
    {
        await playwrightService.StartScrapping(addressId: addressId, continueUntilPrevious: continueUntilPrevious, ct: ct);
    }
}