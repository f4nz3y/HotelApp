using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HotelApp.DAL
{
    public class HotelDbContextFactory : IDesignTimeDbContextFactory<HotelDbContext>
    {
        public HotelDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<HotelDbContext>();

            optionsBuilder.UseSqlite("Data Source=hotel.db");

            return new HotelDbContext(optionsBuilder.Options);
        }
    }
}