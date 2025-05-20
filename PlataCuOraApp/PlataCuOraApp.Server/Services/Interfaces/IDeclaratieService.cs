using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Services
{
    public interface IDeclaratieService
    {
        Task<byte[]> GenereazaDeclaratieAsync(string userId, List<DateTime> zileLucrate);
    }
}
