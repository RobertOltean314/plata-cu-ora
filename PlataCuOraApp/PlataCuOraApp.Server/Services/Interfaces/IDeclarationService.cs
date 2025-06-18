using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Services
{
    public interface IDeclarationService
    {
        Task<byte[]> GenerateDeclarationAsync(string userId, List<DateTime> workedDays);
    }
}
