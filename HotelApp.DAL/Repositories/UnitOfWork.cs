using HotelApp.DAL.Entities;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Repositories;

namespace HotelApp.DAL.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly HotelDbContext _context;
        private bool _disposed;

        public IRepository<Room> RoomRepository { get; }
        public IRepository<RoomCategory> CategoryRepository { get; }
        public IRepository<Booking> BookingRepository { get; }

        public UnitOfWork(HotelDbContext context)
        {
            _context = context;
            RoomRepository = new Repository<Room>(_context);
            CategoryRepository = new Repository<RoomCategory>(_context);
            BookingRepository = new Repository<Booking>(_context);
        }

        public void Save() => _context.SaveChanges();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}