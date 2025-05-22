using PlataCuOraApp.Server.Domain.DTO;

namespace PlataCuOraApp.Server.Services
{
    public interface IParitateSaptService
    {
        Task AddOrUpdateParitateSaptAsync(string parId, List<ParitateSaptamanaDTO> saptamani);
        Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string parId);
    }
}
