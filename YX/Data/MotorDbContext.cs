using Microsoft.EntityFrameworkCore;

public class MotorDbContext : DbContext
{
    public MotorDbContext(DbContextOptions<MotorDbContext> options) : base(options)
    {
    }

    public DbSet<MotorModel> Motors { get; set; } = null!;
    public DbSet<MotorDataPoint> DataPoints { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<MotorModel>().HasKey(m => m.Id);
        modelBuilder.Entity<MotorDataPoint>().HasKey(d => d.Id);
        modelBuilder.Entity<MotorDataPoint>()
            .HasOne<MotorModel>()
            .WithMany()
            .HasForeignKey(d => d.MotorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
