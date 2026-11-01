namespace HappyPet.Application.Dtos.Responises
{
    public class AuthResult
    {
        public LoginResponse Response { get; set; } = new();

        public string RefreshToken { get; set; } = string.Empty;

        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
