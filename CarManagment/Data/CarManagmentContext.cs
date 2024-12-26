

using CarManagment.Models;
using Microsoft.EntityFrameworkCore;

namespace CarManagment.Data
{
    public partial class CarManagmentContext : DbContext
    {
        public CarManagmentContext()
        {
        }

        public CarManagmentContext(DbContextOptions<CarManagmentContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Maintenance> Maintenances { get; set; }
        public virtual DbSet<Garage> Garages { get; set; }
        public virtual DbSet<Car> Cars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Car>()
                .HasMany(c => c.Garages)
                .WithMany(g => g.Cars)
                .UsingEntity(j => j.ToTable("CarGarage"));

            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Garage)
                .WithMany(g => g.Maintenances)
                .HasForeignKey(m => m.GarageId);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
