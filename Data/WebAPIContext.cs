using Microsoft.EntityFrameworkCore;
using Running_DistanceCaltulate.Entity;

namespace Running_DistanceCaltulate.Data;

public partial class WebAPIContext : DbContext
{
    public WebAPIContext()
    {
    }

    public WebAPIContext(DbContextOptions<WebAPIContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseSerialColumns();
    }

    public virtual DbSet<User> Users { get; set; } = null!;
    public virtual DbSet<CalculationResultDistance> CalculationResult => Set<CalculationResultDistance>();
}