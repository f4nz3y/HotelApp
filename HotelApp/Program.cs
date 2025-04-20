using HotelApp.DAL;
using HotelApp.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddDbContext<HotelDbContext>(options =>
    options.UseSqlite("Data Source=hotel.db"));

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
    Console.WriteLine("\nОБЕРIТЬ ОПЕРАЦІЮ:");
    Console.WriteLine("1. Показати доступнi номери");
    Console.WriteLine("2. Забронювати номер");
    Console.WriteLine("3. Зняти бронювання");
    Console.WriteLine("0. Вийти");

    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            var availableRooms = service.GetAvailableRooms(DateTime.Today, DateTime.Today.AddDays(3));
            foreach (var room in availableRooms)
                Console.WriteLine($"ID: {room.Id}, Номер: {room.Number}, Категорiя: {room.Category.Name}, Ціна: {room.Category.BasePrice} грн");
            break;

        case "2":
            Console.Write("Введiть ID номеру для бронювання: ");
            int roomId = int.Parse(Console.ReadLine()!);
            service.BookRoom(roomId, DateTime.Today, DateTime.Today.AddDays(2));
            Console.WriteLine("Номер заброньовано.");
            break;

        case "3":
            Console.Write("Введiть ID номеру для зняття бронi: ");
            int cancelId = int.Parse(Console.ReadLine()!);
            service.CancelBooking(cancelId);
            Console.WriteLine("Бронювання знято.");
            break;

        case "0":
            running = false;
            break;

        default:
            Console.WriteLine("Невiдома команда.");
            break;
    }
}