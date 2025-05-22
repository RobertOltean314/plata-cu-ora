using System.Collections.Generic;
using System.Threading.Tasks;
using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Repositories
{
    public interface IParitateSaptRepository
    {
        Task AddOrUpdateParitateSaptAsync(string userId, List<ParitateSaptamanaDTO> saptamani);
        Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string userId);
        Task<bool> UpdateParitateAsync(string userId, ParitateSaptamanaDTO oldEntry, ParitateSaptamanaDTO newEntry);
        Task<bool> DeleteParitateAsync(string userId, ParitateSaptamanaDTO entry);
    }
}