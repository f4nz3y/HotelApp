using Microsoft.EntityFrameworkCore;
using HotelApp.DAL.Entities;

namespace HotelApp.DAL;

public class HotelDbContext : DbContext
{
    public DbSet<Room> Rooms { get; set; }
    public DbSet<RoomCategory> Categories { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    public HotelDbContext(DbContextOptions<HotelDbContext> options)
    : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>()
            .HasOne(r => r.Category)
            .WithMany(c => c.Rooms)
            .HasForeignKey(r => r.CategoryId);

        modelBuilder.Entity<Booking>()
            .HasOne(b => b.Room)
            .WithMany(r => r.Bookings)
            .HasForeignKey(b => b.RoomId);
    }
}