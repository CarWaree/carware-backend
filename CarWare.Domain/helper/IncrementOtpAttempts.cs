using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace CarWare.Domain.helper
{
    public static class IncrementOtpAttempts
    {
        public static async Task IncrementOtpAttemptsAsync(
            IDistributedCache cache,
            string attemptsKey,
            int currentAttempts,
            int otpValidityMinutes)
        {
            await cache.SetStringAsync(
                attemptsKey,
                (currentAttempts + 1).ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(otpValidityMinutes)
                });
        }

    }
}
