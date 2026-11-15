using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Domain.Entities;
using HappyPet.Domain.Enums;
using HappyPet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class ReminderRepository : IReminderRepository
    {
        private readonly HappyPetDbContext _context;

        public ReminderRepository(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(VaccinationReminder reminder, CancellationToken cancellationToken = default)
        {
            await _context.VaccinationReminders.AddAsync(reminder, cancellationToken);
        }

        public async Task<VaccinationReminder?> GetAsync(
            int vaccinationId,
            DateTime dueAt,
            int offsetDays,
            ReminderChannel channel,
            CancellationToken cancellationToken = default)
        {
            return await _context.VaccinationReminders
                .FirstOrDefaultAsync(
                    r => r.VaccinationId == vaccinationId
                         && r.DueAt == dueAt
                         && r.OffsetDays == offsetDays
                         && r.Channel == channel,
                    cancellationToken);
        }

        public async Task<(List<VaccinationReminder> Items, int TotalCount)> GetByOwnerPagedAsync(
            int ownerUserId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.VaccinationReminders
                .AsNoTracking()
                .Where(r => r.OwnerUserId == ownerUserId);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.SentAt ?? r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(List<VaccinationReminder> Items, int TotalCount)> GetByPetPagedAsync(
            int petId,
            int skip,
            int take,
            CancellationToken cancellationToken = default)
        {
            var query = _context.VaccinationReminders
                .AsNoTracking()
                .Where(r => r.PetId == petId);

            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.SentAt ?? r.CreatedAt)
                .ThenByDescending(r => r.Id)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
