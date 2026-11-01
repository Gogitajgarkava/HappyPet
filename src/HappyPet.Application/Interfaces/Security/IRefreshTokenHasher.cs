namespace HappyPet.Application.Interfaces.Security
{
    public interface IRefreshTokenHasher
    {
        string Hash(string token);
        bool Verify(string token, string hash);
    }
}
