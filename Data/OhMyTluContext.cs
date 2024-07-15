using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace OhMyTLU.Data;

public partial class OhMyTluContext : DbContext
{
    private readonly IHttpContextAccessor _contextAccessor;
    public OhMyTluContext(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public OhMyTluContext(DbContextOptions<OhMyTluContext> options, IHttpContextAccessor contextAccessor)
        : base(options)
    {
        _contextAccessor = contextAccessor;
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Friend> Friends { get; set; }
    public virtual DbSet<UserSession> UserSessions { get; set; }
    public virtual DbSet<ActionAudit> ActionAudits { get; set; }
    public virtual DbSet<Message> Messages { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.ToTable("User");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<Friend>(entity =>
        {
            // Cấu hình Id là khóa chính và tự tăng
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                  .ValueGeneratedOnAdd(); // Cấu hình tự tăng

            // Cấu hình UserId
            entity.Property(e => e.UserId)
                  .IsRequired()
                  .HasMaxLength(36);

            // Cấu hình FriendId
            entity.Property(e => e.FriendId)
                  .IsRequired()
                  .HasMaxLength(36);
        });
        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.StartTime).IsRequired();
            entity.Property(e => e.EndTime);
            entity.Property(e => e.DeviceId).IsRequired();
        });

        modelBuilder.Entity<ActionAudit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserSessionId).IsRequired();
            entity.HasOne(e => e.UserSession)
                  .WithMany()
                  .HasForeignKey(e => e.UserSessionId);
            entity.Property(e => e.Action).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.Details);
        });

        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.Property(e => e.Id).HasMaxLength(36);
            entity.Property(e => e.SenderId).HasMaxLength(36);
            entity.Property(e => e.ReceiverId).HasMaxLength(36);

            entity.HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        });
            
        OnModelCreatingPartial(modelBuilder);
    }
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        //var modifiedEntities = ChangeTracker.Entries()
        //.Where(e => e.State == EntityState.Added
        //|| e.State == EntityState.Modified
        //|| e.State == EntityState.Deleted)
        //.ToList();

        //foreach (var modifiedEntity in modifiedEntities)
        //{
        //    var auditLog = new ActionAudit
        //    {
        //        UserSessionId = Convert.ToInt32(_contextAccessor.HttpContext.Session.GetString("UserSessionId")),
        //        Action = modifiedEntity.State.ToString(),
        //        Timestamp = DateTime.UtcNow,
        //        Details = GetChanges(modifiedEntity)
        //    };
        //    ActionAudits.Add(auditLog);
        //}
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
    private static string GetChanges(EntityEntry entity)
    {
        var changes = new StringBuilder();
        foreach (var property in entity.OriginalValues.Properties)
        {
            var originalValue = entity.OriginalValues[property];
            var currentValue = entity.CurrentValues[property];
            if (!Equals(originalValue, currentValue))
            {
                changes.AppendLine($"{property.Name}: From '{originalValue}' to '{currentValue}'");
            }
        }
        return changes.ToString();
    }
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
