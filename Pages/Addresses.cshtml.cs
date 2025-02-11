using MachineTrading.Models;
using MachineTrading.Repository.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MachineTrading.Pages;

public class AddressesModel(IAddressRepo addressRepo) : PageModel
{
    public List<Address>? Addresses { get; set; }
    public async Task OnGet(CancellationToken ct = default)
    {
        Addresses = await addressRepo.GetAll(ct);
    }

    public async Task OnPost(Address address, CancellationToken ct = default)
    {
        await addressRepo.AddAsync(address, ct);
        await addressRepo.SaveChangesAsync(ct);
    }

    public async Task OnGetDelete(int addressId, CancellationToken ct = default)
    {
        var address = await addressRepo.FindAsync(addressId, ct);
        addressRepo.Remove(address!);
        await addressRepo.SaveChangesAsync(ct);
    }
}