using HappyPet.Application.Interfaces.Code;
using System.Security.Cryptography;

namespace HappyPet.Infrastructure.ExentialServices
{
    public class CodeGenerator : ICodeGenerator
    {
        public string GenerateCode(int length = 6)
        {
            if (length < 1 || length > 9)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "კოდის სიგრძე უნდა იყოს 1-დან 9-მდე.");
            }

            var min = (int)Math.Pow(10, length - 1);
            var max = (int)Math.Pow(10, length);

            return RandomNumberGenerator.GetInt32(min, max).ToString();
        }
    }
}
