using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations
{
    public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.Property(a => a.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(a => a.Description)
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(a => a.Location)
                .HasMaxLength(500);

            builder.Property(a => a.MeetingLink)
                .HasMaxLength(1000);

            builder.Property(a => a.CancellationReason)
                .HasMaxLength(1000);

            // Foreign key relationship
            builder.HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for faster querying
            builder.HasIndex(a => a.StartTime);
            builder.HasIndex(a => a.EndTime);
            builder.HasIndex(a => a.UserId);
        }
    }
} 