using EndpointEntities.Models;
using Microsoft.EntityFrameworkCore;

namespace EndpointEntities;

public partial class PushSubscriptionContext : DbContext
{
    public PushSubscriptionContext()
    {
    }

    public PushSubscriptionContext(DbContextOptions<PushSubscriptionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PushSubscription> PushSubscription { get; set; }

//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
// #warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
//         => optionsBuilder.UseSqlServer("Server=(local);Database=db_web_subscription;User Id=sa;Password=db+123456;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Turkish_CI_AS");

        modelBuilder.Entity<PushSubscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PushSubs__3214EC0728A3511D");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Auth).HasColumnType("text");
            entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Endpoint).HasColumnType("text");
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValueSql("((1))");
            entity.Property(e => e.P256dh).HasColumnType("text");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
