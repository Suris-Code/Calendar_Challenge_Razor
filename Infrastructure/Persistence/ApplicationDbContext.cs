using Application.Common.Interfaces;
using Domain.Common;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel;
using System.Reflection;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    #region DbSets

    // Logging
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<UserActivityLog> UserActivityLogs { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<EmailLog> EmailLogs { get; set; }
    
    // Features
    public DbSet<Appointment> Appointments { get; set; }

    #endregion

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService, IDateTime dateTime) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
            {
                string userId = _currentUserService.UserId ?? string.Empty;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = userId;
                        entry.Entity.Created = _dateTime.UtcNow;
                        entry.Entity.LastModifiedBy = userId;
                        entry.Entity.LastModified = _dateTime.UtcNow;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedBy = userId;
                        entry.Entity.LastModified = _dateTime.UtcNow;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            DbUpdateException? dbUpdateEx = e as DbUpdateException;

            if (dbUpdateEx?.InnerException is SqlException sqlEx)
            {
                return sqlEx.Number switch
                {
                    547 => throw new Exception(MessageFKenInsertUpdateDelete(sqlEx.Message, dbUpdateEx.Entries[0].Entity)),
                    2601 => throw new Exception(MessageDuplicateKey(sqlEx.Message)),
                    2627 => throw new Exception(MessageDuplicateKey(sqlEx.Message)),
                    515 => throw new Exception(sqlEx.Message),
                    _ => throw new Exception(sqlEx.Message),
                };
            }

            throw;
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);

        #region Default Values
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var memberInfo = property.PropertyInfo ?? (MemberInfo)property.FieldInfo;
                if (memberInfo == null) continue;
                var defaultValue = Attribute.GetCustomAttribute(memberInfo, typeof(DefaultValueAttribute)) as DefaultValueAttribute;
                if (defaultValue == null) continue;
                property.SetDefaultValueSql(defaultValue.Value.ToString());
            }
        }
        #endregion

        #region Unique Constraints

        #endregion

        #region Assign all Foreign Keys as DELETE RESTRICT (default behaviour is CASCADE)
        foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
        #endregion
    }

    private string MessageDuplicateKey(string originalMessage)
    {
        return "Cannot insert a duplicate value. Duplicate value: " + originalMessage.Split('(', ')')[1];
    }

    private string MessageFKenInsertUpdateDelete(string originalMessage, object entity)
    {
        string message;
        var tableName = originalMessage.Split("table \"")[1].Split("'")[0].Replace("\"", "").Replace(", column", "").Replace(" ", "").Replace("dbo.", "");

        var words = originalMessage.Split(" ");
        switch (words[1].ToUpper())
        {
            case "INSERT":
                message = String.Format("It is not possible to create this element because some value of the form is not present in {0}.", tableName);
                break;
            case "UPDATE":
                message = "It is not possible to update this element because some value that is being modified is not present in other related elements.";
                break;
            case "DELETE":
                message = String.Format("It is not possible to delete this element because it is related to other elements on the system.");
                break;
            default:
                return originalMessage;
        }

        return message;
    }
}
