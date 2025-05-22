using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Repositories
{
    public interface IParitateSaptRepository
    {
        Task AddOrUpdateParitateSaptAsync(string parId, List<ParitateSaptamanaDTO> saptamani);
        Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string parId);
    }
}
