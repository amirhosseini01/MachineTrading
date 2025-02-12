using MachineTrading.Data;
using MachineTrading.Models;
using MachineTrading.Repository.Base;
using MachineTrading.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MachineTrading.Repository.Implementations;

public class AddressRepo(MachineTradingContext context): GenericRepository<Address>(context), IAddressRepo
{
    private readonly DbSet<Address> _store = context.Addresses;
    public async Task<List<Address>> GetAll(CancellationToken ct = default)
    {
        return await _store.OrderBy(x=> x.Id).ToListAsync(ct);
    }

    public async Task<bool> IsUrlExist(string url)
    {
        return await _store.AnyAsync(x => x.Url == url.ToLower());
    }
}