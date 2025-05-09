using FirebaseAdmin.Auth;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace PlataCuOraApp.Server.Infrastructure.Helpers
{
    public class FirebaseTokenValidator
    {
        private readonly FirebaseAuth _firebaseAuth;
        private readonly ILogger<FirebaseTokenValidator> _logger;

        public FirebaseTokenValidator(FirebaseAuth firebaseAuth, ILogger<FirebaseTokenValidator> logger)
        {
            _firebaseAuth = firebaseAuth;
            _logger = logger;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var decodedToken = await _firebaseAuth.VerifyIdTokenAsync(token);
                return decodedToken != null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token validation failed");
                return false;
            }
        }
    }
}