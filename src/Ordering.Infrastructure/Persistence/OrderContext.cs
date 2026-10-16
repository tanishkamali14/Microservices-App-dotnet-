using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence;

public class OrderContext : DbContext
{
    public DbSet<Order> Orders { get; set; } = null!;

    public OrderContext(DbContextOptions<OrderContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Tables for MassTransit's transactional inbox/outbox. The inbox records every message
        // this service has consumed, in the same transaction as the order it created, so a
        // redelivered checkout message is recognised and skipped instead of creating a duplicate.
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedDate = DateTime.UtcNow;
                    entry.Entity.CreatedBy = "admin";
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedDate = DateTime.UtcNow;
                    entry.Entity.LastModifiedBy = "admin";
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
