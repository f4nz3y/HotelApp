using HotelApp.DAL;
using HotelApp.Core.Services;
using HotelApp.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDbContext<HotelDbContext>(options =>
    options.UseLazyLoadingProxies()
           .UseSqlite("Data Source=hotel.db"));

services.AddTransient<RoomService>();

var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
context.Database.EnsureCreated();

var service = scope.ServiceProvider.GetRequiredService<RoomService>();
service.SeedData();

bool running = true;
while (running)
{
    Console.WriteLine("\nОБЕРIТЬ ОПЕРАЦIЮ:");
    Console.WriteLine("1. Показати доступнi номери (Eager Loading)");
    Console.WriteLine("2. Показати доступнi номери (Lazy Loading)");
    Console.WriteLine("3. Забронювати номер");
    Console.WriteLine("4. Скасувати бронювання");
    Console.WriteLine("5. Показати всi номери");
    Console.WriteLine("6. Додати номер");
    Console.WriteLine("7. Оновити номер");
    Console.WriteLine("8. Видалити номер");
    Console.WriteLine("0. Вийти");

    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            var eagerRooms = context.Rooms
                .Include(r => r.Category)
                .Include(r => r.Bookings)
                .Where(r => r.Status == RoomStatus.Available)
                .ToList();

            Console.WriteLine("[EAGER] Доступнi номери:");
            foreach (var room1 in eagerRooms)
            {
                Console.WriteLine($"ID: {room1.Id}, Номер: {room1.Number}, Категорiя: {room1.Category.Name}, " +
                                  $"Цiна: {room1.Category.BasePrice} грн, Статус: {room1.Status}");
            }
            if (!eagerRooms.Any())
            {
                Console.WriteLine("На жаль, вiльних номерiв немає.");
            }
            break;

        case "2":
            var lazyRooms = context.Rooms
                .Where(r => r.Status == RoomStatus.Available)
                .ToList();

            Console.WriteLine("[LAZY] Доступнi номери:");
            foreach (var room1 in lazyRooms)
            {
                Console.WriteLine($"ID: {room1.Id}, Номер: {room1.Number}, Категорiя: {room1.Category.Name}, " +
                                  $"Цiна: {room1.Category.BasePrice} грн, Статус: {room1.Status}");
            }
            if (!lazyRooms.Any())
            {
                Console.WriteLine("На жаль, вiльних номерiв немає.");
            }
            break;

        case "3":
            Console.Write("ID номеру для бронювання: ");
            int roomId = int.Parse(Console.ReadLine()!);
            Console.Write("Початкова дата (yyyy-MM-dd): ");
            var start = DateTime.Parse(Console.ReadLine()!);
            Console.Write("Кiнцева дата (yyyy-MM-dd): ");
            var end = DateTime.Parse(Console.ReadLine()!);
            Console.Write("Iм’я клiєнта: ");
            var name = Console.ReadLine()!;

            var (room, totalPrice, booking) = service.PreviewBooking(roomId, start, end, name);
            if (booking == null)
            {
                Console.WriteLine("Бронювання неможливе. Перевiрте данi або доступнiсть.");
                break;
            }

            Console.WriteLine($"Пiдтвердити бронювання {room!.Number} з {start:dd.MM} по {end:dd.MM} за {totalPrice} грн? (y/n)");
            if (Console.ReadLine()!.ToLower() == "y")
            {
                service.ConfirmBooking(booking);
                Console.WriteLine("Номер заброньовано.");
            }
            else
            {
                Console.WriteLine("Бронювання скасовано.");
            }
            break;

        case "4":
            Console.Write("ID номеру для зняття бронi: ");
            int cancelId = int.Parse(Console.ReadLine()!);
            service.CancelBooking(cancelId);
            Console.WriteLine("Бронювання скасовано.");
            break;

        case "5":
            var allRooms = service.GetAllRooms();
            foreach (var r in allRooms)
                Console.WriteLine($"ID: {r.Id}, Номер: {r.Number}, Статус: {r.Status}, Категорiя: {r.Category.Name}");
            break;

        case "6":
            Console.Write("Номер кiмнати: ");
            string number = Console.ReadLine()!;
            Console.Write("ID категорiї: ");
            int categoryId = int.Parse(Console.ReadLine()!);
            service.AddRoom(new Room { Number = number, Status = RoomStatus.Available, CategoryId = categoryId });
            Console.WriteLine("Кiмнату додано.");
            break;

        case "7":
            Console.Write("ID кiмнати для оновлення: ");
            int updateId = int.Parse(Console.ReadLine()!);
            var roomToUpdate = service.GetRoom(updateId);
            if (roomToUpdate == null)
            {
                Console.WriteLine("Кiмнату не знайдено.");
                break;
            }
            Console.Write("Новий номер кiмнати: ");
            roomToUpdate.Number = Console.ReadLine()!;
            Console.Write("Новий ID категорiї: ");
            roomToUpdate.CategoryId = int.Parse(Console.ReadLine()!);
            Console.Write("Новий статус (0: Available, 1: Booked, 2: Occupied): ");
            roomToUpdate.Status = (RoomStatus)int.Parse(Console.ReadLine()!);
            service.UpdateRoom(roomToUpdate);
            Console.WriteLine("Кiмнату оновлено.");
            break;

        case "8":
            Console.Write("ID кiмнати для видалення: ");
            int deleteId = int.Parse(Console.ReadLine()!);
            service.DeleteRoom(deleteId);
            Console.WriteLine("Кiмнату видалено.");
            break;

        case "0":
            running = false;
            break;

        default:
            Console.WriteLine("Невiдома команда.");
            break;
    }
}
