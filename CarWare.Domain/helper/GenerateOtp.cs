using System;
using System.Security.Cryptography;

namespace CarWare.Domain.Helpers
{
    public static class OtpGenerator
    {
        public static string Generate()
        {
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            return (BitConverter.ToUInt32(bytes, 0) % 900000 + 100000).ToString();
        }
    }
}