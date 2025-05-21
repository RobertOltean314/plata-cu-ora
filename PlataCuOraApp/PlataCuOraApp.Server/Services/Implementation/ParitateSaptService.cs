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

        public async Task AddOrUpdateParitateSaptAsync(string userId, List<ParitateSaptamanaDTO> saptamani)
        {
            await _repository.AddOrUpdateParitateSaptAsync(userId, saptamani);
        }

        public async Task<List<ParitateSaptamanaDTO>> GetParitateSaptAsync(string userId)
        {
            return await _repository.GetParitateSaptAsync(userId);
        }

        public async Task<bool> UpdateParitateAsync(string userId, ParitateSaptamanaDTO oldEntry, ParitateSaptamanaDTO newEntry)
        {
            return await _repository.UpdateParitateAsync(userId, oldEntry, newEntry);
        }

        public async Task<bool> DeleteParitateAsync(string userId, ParitateSaptamanaDTO entry)
        {
            return await _repository.DeleteParitateAsync(userId, entry);
        }
    }
}
