namespace HappyPet.Application.Interfaces.Security
{
    public interface IOtpHasher
    {
        string Hash(string code);
        bool Verify(string code, string hash);
    }
}
