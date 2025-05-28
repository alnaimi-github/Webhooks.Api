using Microsoft.EntityFrameworkCore;
using Webhooks.Api.Models;

namespace Webhooks.Api.Data;

public class WebhookDbContext : DbContext
{
    public DbSet<WebhookSubscription> WebhookSubscriptions { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
     public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts { get; set; } = null!;

    public WebhookDbContext(DbContextOptions<WebhookDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("Subscriptions", "webhooks");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.EventType)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(s => s.WebhookUrl)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(s => s.CreateOnUtc)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        });


        modelBuilder.Entity<Order>(builder =>
        {
            builder.ToTable("Orders");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.CustomerName)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(o => o.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
            builder.Property(o => o.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        });


        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("WebhookDeliveryAttempts", "webhooks");
            builder.HasKey(a => a.Id);
            builder.Property(a => a.WebhookSubscriptionId)
                .IsRequired();
            builder.Property(a => a.EventType)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne<WebhookSubscription>()
                .WithMany()
                .HasForeignKey(a => a.WebhookSubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
