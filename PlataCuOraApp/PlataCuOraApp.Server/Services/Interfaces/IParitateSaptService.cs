using PlataCuOraApp.Server.Domain.DTO;
using PlataCuOraApp.Server.Repositories;
using PlataCuOraApp.Server.Services;

namespace PlataCuOraApp.Server.Services
{
    public class ParitateSaptService : IParitateSaptService
    {
        private readonly IParitateSaptRepository _repository;

        public ParitateSaptService(IParitateSaptRepository repository)
        {
            _repository = repository;
        }

        public async Task AddOrUpdateParitateSaptAsync(string parId, List<ParitateSaptamanaDTO> saptamani)
        {
            await _repository.AddOrUpdateParitateSaptAsync(parId, saptamani);
        }

        public async Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string parId)
        {
            return await _repository.GetParitateSaptAsync(parId);
        }
    }
}
