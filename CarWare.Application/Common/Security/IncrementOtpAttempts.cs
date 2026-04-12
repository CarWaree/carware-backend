using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace CarWare.Application.Common.Security
{
    public static class OtpAttemptManager
    {
        public static async Task<int> IncrementAsync(
            IDistributedCache cache,
            string key,
            int currentAttempts,
            int validityMinutes)
        {
            currentAttempts++;

            await cache.SetStringAsync(
                key,
                currentAttempts.ToString(),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(validityMinutes)
                });

            return currentAttempts;
        }

        public static async Task ResetAsync(IDistributedCache cache, params string[] keys)
        {
            foreach (var key in keys)
                await cache.RemoveAsync(key);
        }
    }
}