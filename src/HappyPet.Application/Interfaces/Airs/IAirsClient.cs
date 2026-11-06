using HappyPet.Application.Dtos.Airs;
using System.Threading;
using System.Threading.Tasks;

namespace HappyPet.Application.Interfaces.Airs
{
    public interface IAirsClient
    {
        Task<AirsAnimalDetails?> GetAnimalByMicrochipAsync(
            string microchipNumber,
            CancellationToken cancellationToken = default);
    }
}
