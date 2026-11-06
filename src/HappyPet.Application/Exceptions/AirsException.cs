using System;

namespace HappyPet.Application.Exceptions
{
    public abstract class AirsException : Exception
    {
        public string Code { get; }

        protected AirsException(string code, string message) : base(message)
        {
            Code = code;
        }

        protected AirsException(string code, string message, Exception innerException)
            : base(message, innerException)
        {
            Code = code;
        }
    }

    public sealed class AirsAnimalNotFoundException : AirsException
    {
        public AirsAnimalNotFoundException(string message)
            : base("AIRS_MICROCHIP_NOT_FOUND", message)
        {
        }
    }

    public sealed class AirsOwnerMismatchException : AirsException
    {
        public AirsOwnerMismatchException(string message)
            : base("AIRS_OWNER_MISMATCH", message)
        {
        }
    }

    public sealed class AirsUnavailableException : AirsException
    {
        public AirsUnavailableException(string message)
            : base("AIRS_UNAVAILABLE", message)
        {
        }

        public AirsUnavailableException(string message, Exception innerException)
            : base("AIRS_UNAVAILABLE", message, innerException)
        {
        }
    }
}
