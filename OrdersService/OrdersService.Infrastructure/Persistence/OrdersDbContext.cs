using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OrdersService.Domain.Entities;
using OrdersService.Domain.ValueObjects;

namespace OrdersService.Infrastructure.Persistence;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessage> OutboxMessages {get; init;}
    public DbSet<Order> Orders {get; init;}
    public DbSet<InboxMessage> InboxMessages { get; init; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CreateInboxMessagesDb(modelBuilder);
        CreateOutboxMessageDb(modelBuilder);
        CreateOrderDb(modelBuilder);
    }
    
    private void CreateInboxMessagesDb(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InboxMessage>(entity =>
        {
            var statusConverter = new ValueConverter<InboxMessageStatus, short>(
                status => (short)status,
                status => (InboxMessageStatus)status
            );
            
            entity.ToTable("inbox_messages");
            
            entity.HasKey(x => x.Id);
            
            entity.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(x => x.Status)
                .HasConversion(statusConverter)
                .HasColumnName("status")
                .IsRequired();
            
            entity.Property(x => x.Content)
                .HasColumnName("content")
                .IsRequired();
            
            entity.Property(x => x.Type)
                .HasColumnName("type")
                .IsRequired();
        });
    }

    private void CreateOutboxMessageDb(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            var statusConverter = new ValueConverter<OutboxMessageStatus, short>(
                status => (short)status,
                status => (OutboxMessageStatus)status);
            
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);

            entity.Property(x => x.Id)
                .HasColumnName("message_id")
                .IsRequired();
            
            entity.Property(x => x.Type)
                .HasColumnName("type")
                .IsRequired();
            
            entity.Property(x => x.Content)
                .HasColumnName("content")
                .IsRequired();

            entity.Property(x => x.Status)
                .HasConversion(statusConverter)
                .HasColumnName("status")
                .IsRequired();
        });
    }

    private void CreateOrderDb(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            var statusConverter = new ValueConverter<OrderStatus, short>(
                status => (short)status,
                status => (OrderStatus)status
            );

            entity.Property(x => x.Id)
                .HasColumnName("id")
                .IsRequired();
                
            entity.Property(x => x.UserId)
                .HasColumnName("user_id")
                .IsRequired();
                
            entity.Property(x => x.Amount)
                .HasColumnName("amount")
                .IsRequired();
            
            entity.Property(x => x.Description)
                .HasColumnName("description")
                .IsRequired();
                
            entity.Property(x => x.Status)
                .HasColumnName("status")
                .HasConversion(statusConverter)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();
        });
    }
}