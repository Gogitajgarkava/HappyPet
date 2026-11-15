using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;

namespace HappyPet.Application.Interfaces.Repositories
{
    public interface IReminderRepository
    {
        Task AddAsync(VaccinationReminder reminder, CancellationToken cancellationToken = default);

        Task<VaccinationReminder?> GetAsync(
            int vaccinationId,
            DateTime dueAt,
            int offsetDays,
            ReminderChannel channel,
            CancellationToken cancellationToken = default);

        Task<(List<VaccinationReminder> Items, int TotalCount)> GetByOwnerPagedAsync(
            int ownerUserId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);

        Task<(List<VaccinationReminder> Items, int TotalCount)> GetByPetPagedAsync(
            int petId,
            int skip,
            int take,
            CancellationToken cancellationToken = default);
    }
}
