using System;
using System.Security.Cryptography;

namespace CarWare.Application.Common.helper
{
    public class OtpGenerator
    {
        public string Generate()
        {
            var number = RandomNumberGenerator.GetInt32(100000, 1000000);
            return number.ToString();
        }
    }
}