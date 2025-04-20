using HotelApp.DAL;
using HotelApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace HotelApp.Core.Services;

public class RoomService
{
    private readonly HotelDbContext _context;

    public RoomService(HotelDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Room> GetAvailableRooms(DateTime from, DateTime to)
    {
        return _context.Rooms
            .Include(r => r.Category)
            .Include(r => r.Bookings)
            .Where(r => r.Status == RoomStatus.Available &&
                        !r.Bookings.Any(b => b.StartDate < to && b.EndDate > from))
            .ToList();
    }

    public void BookRoom(int roomId, DateTime from, DateTime to)
    {
        var room = _context.Rooms.Find(roomId);
        if (room == null || room.Status != RoomStatus.Available)
        {
            Console.WriteLine("Номер недоступний.");
            return;
        }

        room.Status = RoomStatus.Booked;
        _context.Bookings.Add(new Booking
        {
            RoomId = roomId,
            StartDate = from,
            EndDate = to
        });

        _context.SaveChanges();
    }

    public void CancelBooking(int roomId)
    {
        var room = _context.Rooms.Include(r => r.Bookings).FirstOrDefault(r => r.Id == roomId);
        if (room == null)
        {
            Console.WriteLine("Номер не знайдено.");
            return;
        }

        var bookings = room.Bookings.ToList();
        _context.Bookings.RemoveRange(bookings);
        room.Status = RoomStatus.Available;

        _context.SaveChanges();
    }

    public void SeedData()
    {
        if (!_context.Categories.Any())
        {
            var standard = new RoomCategory { Name = "Стандарт", BasePrice = 1000 };
            var deluxe = new RoomCategory { Name = "Делюкс", BasePrice = 2000 };

            _context.Categories.AddRange(standard, deluxe);
            _context.SaveChanges();

            _context.Rooms.AddRange(
                new Room { Number = "101", CategoryId = standard.Id, Status = RoomStatus.Available },
                new Room { Number = "102", CategoryId = standard.Id, Status = RoomStatus.Available },
                new Room { Number = "201", CategoryId = deluxe.Id, Status = RoomStatus.Available }
            );
            _context.SaveChanges();
        }
    }
}