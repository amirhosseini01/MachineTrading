using MachineTrading.Data;
using MachineTrading.Models;
using MachineTrading.Repository.Base;
using MachineTrading.Repository.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MachineTrading.Repository.Implementations;

public class ArticleRepo(MachineTradingContext context): GenericRepository<Article>(context), IArticleRepo
{
    private readonly DbSet<Article> _store = context.Articles;
    
    public async Task<List<Article>> GetAll(int takeSize = 10, CancellationToken ct = default)
    {
        return await _store.OrderByDescending(x=> x.Time).Take(takeSize).ToListAsync(ct);
    }

    public async Task<bool> IsExistLink(string link, CancellationToken ct = default)
    {
        return await _store.AnyAsync(x => x.Link == link, ct);
    }
}