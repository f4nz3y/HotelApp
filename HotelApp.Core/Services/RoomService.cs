using HotelApp.Core.Factories;
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

    public IEnumerable<Room> GetAvailableRooms(DateTime date)
    {
        return _context.Rooms
            .Include(r => r.Category)
            .Include(r => r.Bookings)
            .Where(r => r.Status == RoomStatus.Available &&
                       (!r.Bookings.Any() || !r.Bookings.Any(b => b.StartDate <= date && b.EndDate > date)))
            .ToList();
    }

    public (Room? room, decimal totalPrice, Booking? bookingPreview) PreviewBooking(int roomId, DateTime from, DateTime to, string clientName)
    {
        var room = _context.Rooms.Include(r => r.Category).Include(r => r.Bookings).FirstOrDefault(r => r.Id == roomId);

        if (room == null || room.Status != RoomStatus.Available || room.Bookings.Any(b => b.StartDate < to && b.EndDate > from))
            return (null, 0, null);

        int days = (to - from).Days;
        decimal total = days * room.Category.BasePrice;

        var booking = new Booking
        {
            RoomId = roomId,
            StartDate = from,
            EndDate = to,
            ClientName = clientName
        };

        return (room, total, booking);
    }

    public void ConfirmBooking(Booking booking)
    {
        var room = _context.Rooms.Find(booking.RoomId);
        if (room != null)
        {
            room.Status = RoomStatus.Booked;
            _context.Bookings.Add(booking);
            _context.SaveChanges();
        }
    }

    public void CancelBooking(int roomId)
    {
        var room = _context.Rooms.Include(r => r.Bookings).FirstOrDefault(r => r.Id == roomId);
        if (room == null) return;

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

    public IEnumerable<Room> GetAllRooms() => _context.Rooms.Include(r => r.Category).ToList();

    public Room? GetRoom(int id) => _context.Rooms.Include(r => r.Category).FirstOrDefault(r => r.Id == id);

    public void AddRoomViaFactory(IRoomFactory factory, string number)
    {
        var (room, category) = factory.CreateRoom(number);

        var existingCategory = _context.Categories
            .FirstOrDefault(c => c.Name == category.Name);

        if (existingCategory != null)
        {
            room.Category = existingCategory;
        }
        else
        {
            _context.Categories.Add(category);
        }

        _context.Rooms.Add(room);
        _context.SaveChanges();
    }

    public void UpdateRoom(Room room)
    {
        _context.Rooms.Update(room);
        _context.SaveChanges();
    }

    public void DeleteRoom(int id)
    {
        var room = _context.Rooms.Find(id);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            _context.SaveChanges();
        }
    }
}
