using MachineTrading.Models;
using MachineTrading.Repository.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MachineTrading.Pages;

public class AddressesModel(IAddressRepo addressRepo) : PageModel
{
    public List<Address>? Addresses { get; set; }
    public async Task OnGet(CancellationToken ct = default)
    {
        Addresses = await addressRepo.GetAll(ct);
    }

    public async Task<IActionResult> OnPost(Address address, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(address.Url))
        {
            ViewData["ErrorMessage"] = "enter the url";
            return Page();
        }
        
        if (await addressRepo.IsUrlExist(address.Url))
        {
            ViewData["ErrorMessage"] = "url should be unique";
            return Page();
        }

        address.Url = address.Url.ToLower();
        
        await addressRepo.AddAsync(address, ct);
        await addressRepo.SaveChangesAsync(ct);

        return RedirectToPage("Addresses");
    }

    public async Task<IActionResult> OnGetDelete(int addressId, CancellationToken ct = default)
    {
        var address = await addressRepo.FindAsync(addressId, ct);
        addressRepo.Remove(address!);
        await addressRepo.SaveChangesAsync(ct);
        return RedirectToPage("Addresses");
    }
}