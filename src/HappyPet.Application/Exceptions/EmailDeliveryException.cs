namespace HappyPet.Application.Exceptions
{
    public class EmailDeliveryException : Exception
    {
        public EmailDeliveryException(string message)
            : base(message)
        {
        }

        public EmailDeliveryException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
