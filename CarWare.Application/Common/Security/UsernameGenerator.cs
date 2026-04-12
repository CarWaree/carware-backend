using System;

namespace CarWare.Application.Common.Helpers
{
    public static class UsernameGenerator
    {
        public static string Generate(string email)
        {
            var baseName = email.Split('@')[0];
            var random = Random.Shared.Next(1000, 9999);
            return $"{baseName}{random}";
        }
    }
}