using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        public EntityEntry<TEntity> Entry<TEntity>(TEntity entity) where TEntity : class;
        public DbSet<ErrorLog> ErrorLogs { get; set; }
        public DbSet<UserActivityLog> UserActivityLogs { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<EmailLog> EmailLogs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
