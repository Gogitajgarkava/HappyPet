namespace HappyPet.Application.Options
{
    public class JwtOptions
    {
        public string Secret { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;

        public int AccessTokenMinutes { get; set; } = 15;

        public int RefreshTokenDays { get; set; } = 30;

        public string RefreshTokenHashKey { get; set; } = string.Empty;
    }
}
