using MachineTrading.Models;
using Microsoft.EntityFrameworkCore;

namespace MachineTrading.Data;
//  dotnet ef migrations add Init
//  dotnet ef database update
public class MachineTradingContext: DbContext
{
    public MachineTradingContext(DbContextOptions<MachineTradingContext> options): base(options)
    {
        
    }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Selector> Selectors { get; set; }
}