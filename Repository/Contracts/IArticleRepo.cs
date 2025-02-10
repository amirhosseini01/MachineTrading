using MachineTrading.Models;
using MachineTrading.Repository.Base;

namespace MachineTrading.Repository.Contracts;

public interface IArticleRepo: IGenericRepository<Article>
{
    Task<List<Article>> GetAll(int takeSize = 10, CancellationToken ct = default);
    Task<bool> IsExistLink(string link, CancellationToken ct = default);
}