using MachineTrading.Models;
using MachineTrading.Repository.Base;

namespace MachineTrading.Repository.Contracts;

public interface IAddressRepo: IGenericRepository<Address>
{
    Task<List<Address>> GetAll(CancellationToken ct = default);
    Task<bool> IsUrlExist(string url);
}