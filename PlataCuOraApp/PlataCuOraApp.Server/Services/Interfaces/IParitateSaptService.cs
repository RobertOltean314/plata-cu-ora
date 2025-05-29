using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Services
{
    public interface IParitateSaptService
    {
        Task AddOrUpdateParitateSaptAsync(string userId, List<ParitateSaptamanaDTO> saptamani);
        Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string parId);
        Task<bool> UpdateParitateAsync(string userId, ParitateSaptamanaDTO oldEntry, ParitateSaptamanaDTO newEntry);
        Task<bool> DeleteParitateAsync(string userId, ParitateSaptamanaDTO entry);
    }
}
