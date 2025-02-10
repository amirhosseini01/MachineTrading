using MachineTrading.Models;
using MachineTrading.Repository.Base;

namespace MachineTrading.Repository.Contracts;

public interface ISelectorRepo: IGenericRepository<Selector>
{
    Task<List<Selector>> GetAll(CancellationToken ct = default);
}