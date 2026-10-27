namespace HappyPet.Application.Exceptions
{
    public class OtpRateLimitException : InvalidOperationException
    {
        public OtpRateLimitException(string message)
            : base(message)
        {
        }
    }
}
