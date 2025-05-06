using HotelApp.DAL.Entities;

namespace HotelApp.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Room> RoomRepository { get; }
        IRepository<RoomCategory> CategoryRepository { get; }
        IRepository<Booking> BookingRepository { get; }
        void Save();
    }
}