using HappyPet.Application.Dtos.Airs;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Application.Interfaces.Services
{
    public interface IAirsService
    {
        Task<AirsAnimalDetails> GetAnimalByMicrochipAsync(
            string microchipNumber,
            CancellationToken cancellationToken = default);

        Task EnsureMicrochipBelongsToOwnerAsync(
            string microchipNumber,
            string ownerPersonalId,
            CancellationToken cancellationToken = default);
    }
}
