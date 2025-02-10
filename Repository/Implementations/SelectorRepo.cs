using MachineTrading.Data;
using MachineTrading.Models;
using MachineTrading.Repository.Base;
using MachineTrading.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MachineTrading.Repository.Implementations;

public class SelectorRepo(MachineTradingContext context): GenericRepository<Selector>(context), ISelectorRepo
{
    private readonly DbSet<Selector> _store = context.Selectors;
    
    public async Task<List<Selector>> GetAll(CancellationToken ct = default)
    {
        return await _store.OrderBy(x=> x.Id).ToListAsync(ct);
    }
}