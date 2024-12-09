using API.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext : IdentityDbContext<User>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }
    
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Booking> Bookings { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasOne(u => u.DoctorProfile)  
            .WithOne(d => d.User)         
            .HasForeignKey<Doctor>(d => d.UserId) 
            .OnDelete(DeleteBehavior.Cascade);
    }
}
