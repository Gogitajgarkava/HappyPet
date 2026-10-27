namespace HappyPet.Application.Exceptions
{
    public class EmailNotVerifiedException : Exception
    {
        public EmailNotVerifiedException()
            : base("გთხოვთ დაადასტუროთ ელ-ფოსტა.")
        {
        }
    }
}
