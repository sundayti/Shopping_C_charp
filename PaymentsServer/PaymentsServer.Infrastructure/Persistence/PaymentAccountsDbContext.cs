using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentsServer.Domain.Entities;
using PaymentsServer.Domain.ValueObjects;

namespace PaymentsServer.Infrastructure.Persistence;

public class PaymentAccountsDbContext(DbContextOptions<PaymentAccountsDbContext> options) : DbContext(options)
{
    public DbSet<PaymentAccount> PaymentAccounts { get; init; } 
    public DbSet<InboxMessage> InboxMessages { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        CreateInboxMessagesDb(modelBuilder);
        CreatePaymentAccountsDb(modelBuilder);
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

    private void CreatePaymentAccountsDb(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentAccount>(entity =>
        {
            var balanceConverter = new ValueConverter<Balance, decimal>(
                balance => balance.Value,
                balance => new Balance(balance)
            );
            
            entity.ToTable("payment_accounts");

            entity.HasKey(x => x.UserId);
            
            entity.Property(x => x.UserId)
                .HasColumnName("id")
                .IsRequired();
            
            entity.Property(x => x.Balance)
                .HasConversion(balanceConverter)
                .HasColumnName("balance")
                .IsRequired();
        });
    }
}