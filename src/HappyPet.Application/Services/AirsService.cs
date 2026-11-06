using HappyPet.Application.Dtos.Airs;
using HappyPet.Application.Exceptions;
using HappyPet.Application.Interfaces.Airs;
using HappyPet.Application.Interfaces.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Application.Services
{
    public class AirsService : IAirsService
    {
        private readonly IAirsClient _airsClient;

        public AirsService(IAirsClient airsClient)
        {
            _airsClient = airsClient;
        }

        public async Task<AirsAnimalDetails> GetAnimalByMicrochipAsync(
            string microchipNumber,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(microchipNumber))
            {
                throw new AirsAnimalNotFoundException(
                    "მიკროჩიპის ნომერი არ არის მითითებული.");
            }

            var details = await _airsClient.GetAnimalByMicrochipAsync(
                microchipNumber.Trim(),
                cancellationToken);

            if (details == null)
            {
                throw new AirsAnimalNotFoundException(
                    "მითითებული მიკროჩიპის ნომერი AIRS სისტემაში ვერ მოიძებნა. ცხოველის რეგისტრაცია შეუძლებელია.");
            }

            return details;
        }

        public async Task EnsureMicrochipBelongsToOwnerAsync(
            string microchipNumber,
            string ownerPersonalId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ownerPersonalId))
            {
                throw new InvalidOperationException(
                    "მომხმარებლის პირადი ნომერი ვერ მოიძებნა. მიკროჩიპის მფლობელის შემოწმება შეუძლებელია.");
            }

            var details = await GetAnimalByMicrochipAsync(microchipNumber, cancellationToken);
            var airsOwnerPersonalId = details.Animal.Owner?.PersonalId;

            if (string.IsNullOrWhiteSpace(airsOwnerPersonalId) ||
                !string.Equals(
                    airsOwnerPersonalId.Trim(),
                    ownerPersonalId.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new AirsOwnerMismatchException(
                    "მითითებული მიკროჩიპი AIRS სისტემაში სხვა მფლობელზეა რეგისტრირებული. ცხოველის რეგისტრაცია შეუძლებელია.");
            }
        }
    }
}
