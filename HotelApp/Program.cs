using AutoMapper;
using HotelApp.Core.Factories;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Models;
using HotelApp.Core.Services;
using HotelApp.DAL;
using HotelApp.DAL.Entities;
using HotelApp.DAL.Interfaces;
using HotelApp.DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace HotelApp.UI
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();

            services.AddDbContext<HotelDbContext>(options =>
                options.UseSqlite("Data Source=hotel.db"));

            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<Room, RoomModel>()
                   .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                   .ForMember(dest => dest.CategoryBasePrice, opt => opt.MapFrom(src => src.Category.BasePrice))
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

                cfg.CreateMap<RoomModel, Room>();
                cfg.CreateMap<Booking, BookingModel>();
                cfg.CreateMap<BookingModel, Booking>();
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IRoomService, RoomService>();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
            if (context.Database.GetPendingMigrations().Any())
            {
                context.Database.Migrate();
            }

            var service = scope.ServiceProvider.GetRequiredService<IRoomService>();
            service.SeedData();

            bool running = true;
            while (running)
            {
                Console.WriteLine("\nОБЕРIТЬ ОПЕРАЦIЮ:");
                Console.WriteLine("1. Показати доступнi номери");
                Console.WriteLine("2. Забронювати номер");
                Console.WriteLine("3. Скасувати бронювання");
                Console.WriteLine("4. Показати всi номери");
                Console.WriteLine("5. Додати номер");
                Console.WriteLine("6. Оновити номер");
                Console.WriteLine("7. Видалити номер");
                Console.WriteLine("0. Вийти");

                var input = Console.ReadLine();

                try
                {
                    switch (input)
                    {
                        case "1":
                            Console.Write("Введiть дату (yyyy-MM-dd): ");
                            var date = DateTime.Parse(Console.ReadLine());
                            var rooms = service.GetAvailableRooms(date);
                            PrintRooms(rooms);
                            break;

                        case "2":
                            BookRoom(service);
                            break;

                        case "3":
                            Console.Write("ID номеру для зняття бронi: ");
                            int cancelId = int.Parse(Console.ReadLine());
                            service.CancelBooking(cancelId);
                            Console.WriteLine("Бронювання скасовано.");
                            break;

                        case "4":
                            var allRooms = service.GetAllRooms();
                            PrintRooms(allRooms);
                            break;

                        case "5":
                            AddRoom(service);
                            break;

                        case "6":
                            UpdateRoom(service);
                            break;

                        case "7":
                            Console.Write("ID кiмнати для видалення: ");
                            int deleteId = int.Parse(Console.ReadLine());
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
                catch (Exception ex)
                {
                    Console.WriteLine($"Помилка: {ex.Message}");
                }
            }
        }

        static void PrintRooms(IEnumerable<RoomModel> rooms)
        {
            Console.WriteLine("\nСписок номерiв:");
            foreach (var room in rooms)
            {
                Console.WriteLine($"ID: {room.Id}, Номер: {room.Number}, " +
                    $"Категорiя: {room.CategoryName}, Цiна: {room.CategoryBasePrice}, Статус: {room.Status}");
            }
            if (!rooms.Any())
            {
                Console.WriteLine("Номери вiдсутнi.");
            }
        }

        static void BookRoom(IRoomService service)
        {
            Console.Write("ID номеру для бронювання: ");
            int roomId = int.Parse(Console.ReadLine());
            Console.Write("Початкова дата (yyyy-MM-dd): ");
            var start = DateTime.Parse(Console.ReadLine());
            Console.Write("Кiнцева дата (yyyy-MM-dd): ");
            var end = DateTime.Parse(Console.ReadLine());
            Console.Write("Iм'я клiєнта: ");
            var name = Console.ReadLine();

            var (room, totalPrice, booking) = service.PreviewBooking(roomId, start, end, name);
            if (booking == null)
            {
                Console.WriteLine("Бронювання неможливе. Перевiрте данi або доступнiсть.");
                return;
            }

            Console.WriteLine($"Пiдтвердити бронювання {room.Number} з {start:dd.MM} по {end:dd.MM} за {totalPrice} грн? (y/n)");
            if (Console.ReadLine().ToLower() == "y")
            {
                service.ConfirmBooking(booking);
                Console.WriteLine("Номер заброньовано.");
            }
            else
            {
                Console.WriteLine("Бронювання скасовано.");
            }
        }

        static void AddRoom(IRoomService service)
        {
            Console.Write("Номер кiмнати: ");
            string number = Console.ReadLine();

            Console.WriteLine("Оберiть тип кiмнати (1 - Standard, 2 - Deluxe): ");
            var roomType = Console.ReadLine();

            IRoomFactory factory = roomType == "2"
                ? new DeluxeRoomFactory()
                : new StandardRoomFactory();

            service.AddRoomViaFactory(factory, number);
            Console.WriteLine("Кiмнату додано.");
        }

        static void UpdateRoom(IRoomService service)
        {
            Console.Write("ID кiмнати для оновлення: ");
            int updateId = int.Parse(Console.ReadLine());
            var room = service.GetRoom(updateId);
            if (room == null)
            {
                Console.WriteLine("Кiмнату не знайдено.");
                return;
            }

            Console.Write("Новий номер кiмнати (поточний: {0}): ", room.Number);
            var newNumber = Console.ReadLine();
            if (!string.IsNullOrEmpty(newNumber))
                room.Number = newNumber;

            Console.Write("Новий ID категорiї (поточний: {0}): ", room.CategoryId);
            var newCategoryId = Console.ReadLine();
            if (!string.IsNullOrEmpty(newCategoryId))
                room.CategoryId = int.Parse(newCategoryId);

            Console.Write("Новий статус (0: Available, 1: Booked, 2: Occupied, поточний: {0}): ", room.Status);
            var newStatus = Console.ReadLine();
            if (!string.IsNullOrEmpty(newStatus))
                room.Status = ((RoomStatus)int.Parse(newStatus)).ToString();

            service.UpdateRoom(room);
            Console.WriteLine("Кiмнату оновлено.");
        }
    }
}