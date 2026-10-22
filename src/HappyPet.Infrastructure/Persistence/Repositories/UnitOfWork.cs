using System;
using System.Threading;
using System.Threading.Tasks;
using HappyPet.Application.Interfaces.Repositories;
using HappyPet.Infrastructure.Persistence;

namespace HappyPet.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HappyPetDbContext _context;
        private bool _disposed;

        public UnitOfWork(HappyPetDbContext context)
        {
            _context = context;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}