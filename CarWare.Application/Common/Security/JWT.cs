namespace CarWare.Application.Common.Security
{
    public class JWT
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int AccessTokenDurationMinutes { get; set; }
        public int RefreshTokenDurationDays { get; set; }
    }
}