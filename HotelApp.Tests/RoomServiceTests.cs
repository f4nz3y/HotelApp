using AutoFixture;
using AutoMapper;
using HotelApp.Core.Factories;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Models;
using HotelApp.Core.Services;
using HotelApp.DAL.Entities;
using HotelApp.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace HotelApp.Tests
{
    [TestFixture]
    public class RoomServiceTests : IDisposable
    {
        private IRoomService _roomService;
        private IUnitOfWork _unitOfWorkMock;
        private IRepository<Room> _roomRepositoryMock;
        private IRepository<RoomCategory> _categoryRepositoryMock;
        private IRepository<Booking> _bookingRepositoryMock;
        private Fixture _fixture;
        private IMapper _mapper;
        private Func<string, IRoomFactory> _roomFactoryResolverMock;
        private bool _disposed = false;

        [SetUp]
        public void Setup()
        {
            _roomRepositoryMock = Substitute.For<IRepository<Room>>();
            _categoryRepositoryMock = Substitute.For<IRepository<RoomCategory>>();
            _bookingRepositoryMock = Substitute.For<IRepository<Booking>>();

            _unitOfWorkMock = Substitute.For<IUnitOfWork>();
            _unitOfWorkMock.RoomRepository.Returns(_roomRepositoryMock);
            _unitOfWorkMock.CategoryRepository.Returns(_categoryRepositoryMock);
            _unitOfWorkMock.BookingRepository.Returns(_bookingRepositoryMock);

            _fixture = new Fixture();
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _mapper = CreateTestMapper();

            _roomFactoryResolverMock = Substitute.For<Func<string, IRoomFactory>>();

            _roomService = new RoomService(
                _unitOfWorkMock,
                _mapper,
                _roomFactoryResolverMock
            );
        }

        [TearDown]
        public void TearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    (_unitOfWorkMock as IDisposable)?.Dispose();
                }
                _disposed = true;
            }
        }

        [Test]
        public void GetAvailableRooms_ShouldReturnOnlyAvailableRooms()
        {
            var testDate = DateTime.Now.AddDays(1);

            var availableRoom = _fixture.Build<Room>()
                .With(r => r.Status, RoomStatus.Available)
                .With(r => r.Bookings, new List<Booking>())
                .Create();

            var bookedRoom = _fixture.Build<Room>()
                .With(r => r.Status, RoomStatus.Booked)
                .With(r => r.Bookings, new List<Booking>())
                .Create();

            var mockRooms = new List<Room> { availableRoom, bookedRoom };

            _roomRepositoryMock
                .Get(Arg.Any<Expression<Func<Room, bool>>>())
                .Returns(call =>
                {
                    var predicate = call.Arg<Expression<Func<Room, bool>>>().Compile();
                    return mockRooms.Where(predicate).AsQueryable();
                });

            var result = _roomService.GetAvailableRooms(testDate);

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Status, Is.EqualTo("Available"));
        }

        [Test]
        public void PreviewBooking_ShouldReturnCorrectPreview_WhenRoomIsAvailable()
        {
            var roomId = 1;
            var from = DateTime.Now.AddDays(1);
            var to = DateTime.Now.AddDays(3);
            var clientName = "Test Client";

            var room = _fixture.Build<Room>()
                .With(r => r.Id, roomId)
                .With(r => r.Status, RoomStatus.Available)
                .With(r => r.Bookings, new List<Booking>())
                .With(r => r.Category, _fixture.Build<RoomCategory>()
                    .With(c => c.BasePrice, 1000)
                    .Create())
                .Create();

            _unitOfWorkMock.RoomRepository
                .Get(Arg.Any<Expression<Func<Room, bool>>>())
                .Returns(new List<Room> { room }.AsQueryable());

            var (roomModel, totalPrice, bookingPreview) =
                _roomService.PreviewBooking(roomId, from, to, clientName);

            Assert.That(roomModel, Is.Not.Null);
            Assert.That(roomModel.Id, Is.EqualTo(roomId));
            Assert.That(totalPrice, Is.EqualTo(2000));
            Assert.That(bookingPreview.ClientName, Is.EqualTo(clientName));
        }

        [Test]
        public void ConfirmBooking_ShouldAddBookingAndUpdateRoomStatus()
        {
            var bookingModel = _fixture.Build<BookingModel>()
                .With(b => b.StartDate, DateTime.Now.AddDays(1))
                .With(b => b.EndDate, DateTime.Now.AddDays(3))
                .Create();

            var room = _fixture.Build<Room>()
                .With(r => r.Id, bookingModel.RoomId)
                .With(r => r.Status, RoomStatus.Available)
                .Create();

            _unitOfWorkMock.RoomRepository.GetById(bookingModel.RoomId).Returns(room);

            _roomService.ConfirmBooking(bookingModel);

            _unitOfWorkMock.BookingRepository.Received(1).Add(Arg.Any<Booking>());
            _unitOfWorkMock.RoomRepository.Received(1).Update(Arg.Is<Room>(r =>
                r.Status == RoomStatus.Booked));
            _unitOfWorkMock.Received(1).Save();
        }

        [Test]
        public void CancelBooking_ShouldRemoveBookingsAndSetRoomAvailable()
        {
            var roomId = 1;
            var bookings = _fixture.Build<Booking>()
                .With(b => b.RoomId, roomId)
                .CreateMany(2)
                .ToList();

            var room = _fixture.Build<Room>()
                .With(r => r.Id, roomId)
                .With(r => r.Status, RoomStatus.Booked)
                .With(r => r.Bookings, bookings)
                .Create();

            _unitOfWorkMock.RoomRepository
                .Get(Arg.Any<Expression<Func<Room, bool>>>())
                .Returns(new List<Room> { room }.AsQueryable());

            _roomService.CancelBooking(roomId);

            _unitOfWorkMock.BookingRepository.Received(bookings.Count).Delete(Arg.Any<Booking>());
            _unitOfWorkMock.RoomRepository.Received(1).Update(Arg.Is<Room>(r =>
                r.Status == RoomStatus.Available));
            _unitOfWorkMock.Received(1).Save();
        }

        [Test]
        public void GetAllRooms_ShouldReturnAllRooms()
        {
            var rooms = _fixture.Build<Room>()
                .With(r => r.Category, _fixture.Create<RoomCategory>())
                .CreateMany(3)
                .ToList();

            _unitOfWorkMock.RoomRepository.GetAll().Returns(rooms.AsQueryable());

            var result = _roomService.GetAllRooms();

            Assert.That(result.Count(), Is.EqualTo(3));
        }

        [Test]
        public void AddRoomViaFactory_ShouldAddNewRoom_WithExistingCategory()
        {
            var factoryType = "standard";
            var roomNumber = "101";
            var existingCategory = _fixture.Build<RoomCategory>()
                .With(c => c.Name, "Standard")
                .With(c => c.BasePrice, 1000m)  // Додано базову ціну для перевірки
                .Create();

            var newRoom = _fixture.Build<Room>()
                .With(r => r.Number, roomNumber)
                .With(r => r.Status, RoomStatus.Available)
                .With(r => r.Category, existingCategory)  // Додано прив'язку категорії
                .Create();

            var factoryMock = Substitute.For<IRoomFactory>();
            factoryMock.CreateRoom(roomNumber).Returns((newRoom, existingCategory));

            _roomFactoryResolverMock.Invoke(factoryType).Returns(factoryMock);

            _unitOfWorkMock.CategoryRepository
                .Get(Arg.Any<Expression<Func<RoomCategory, bool>>>())
                .Returns(new List<RoomCategory> { existingCategory }.AsQueryable());

            _roomService.AddRoomViaFactory(factoryType, roomNumber);

            _unitOfWorkMock.RoomRepository.Received(1).Add(Arg.Is<Room>(r =>
                r.Number == roomNumber &&
                r.Category.Name == "Standard" &&  // Перевірка типу кімнати
                r.Category.BasePrice == 1000m));  // Перевірка базової ціни

            _unitOfWorkMock.Received(1).Save();
        }

        [Test]
        public void DeleteRoom_ShouldRemoveRoom()
        {
            var roomId = 1;

            _roomService.DeleteRoom(roomId);

            _unitOfWorkMock.RoomRepository.Received(1).Delete(roomId);
            _unitOfWorkMock.Received(1).Save();
        }

        [Test]
        public void SeedData_ShouldAddInitialData_WhenNoCategoriesExist()
        {
            _unitOfWorkMock.CategoryRepository.GetAll().Returns(Enumerable.Empty<RoomCategory>().AsQueryable());

            var standardCategory = new RoomCategory { Id = 1, Name = "Стандарт", BasePrice = 1000 };
            var deluxeCategory = new RoomCategory { Id = 2, Name = "Делюкс", BasePrice = 2000 };

            var standardRooms = new List<Room>
            {
                new Room { Id = 1, Number = "101", CategoryId = 1, Status = RoomStatus.Available },
                new Room { Id = 2, Number = "102", CategoryId = 1, Status = RoomStatus.Available }
            };

            var deluxeRoom = new Room { Id = 3, Number = "201", CategoryId = 2, Status = RoomStatus.Available };

            _roomService.SeedData();

            _unitOfWorkMock.CategoryRepository.Received(1).Add(Arg.Is<RoomCategory>(c => c.Name == "Стандарт"));
            _unitOfWorkMock.CategoryRepository.Received(1).Add(Arg.Is<RoomCategory>(c => c.Name == "Делюкс"));

            _unitOfWorkMock.RoomRepository.Received(1).Add(Arg.Is<Room>(r => r.Number == "101"));
            _unitOfWorkMock.RoomRepository.Received(1).Add(Arg.Is<Room>(r => r.Number == "102"));
            _unitOfWorkMock.RoomRepository.Received(1).Add(Arg.Is<Room>(r => r.Number == "201"));

            _unitOfWorkMock.Received(2).Save();
        }

        [Test]
        public void GetRoom_ShouldReturnCorrectRoom_WhenRoomExists()
        {
            var roomId = 1;
            var expectedRoom = _fixture.Build<Room>()
                .With(r => r.Id, roomId)
                .With(r => r.Category, _fixture.Build<RoomCategory>()
                    .With(c => c.Name, "Стандарт")
                    .With(c => c.BasePrice, 1000)
                    .Create())
                .Create();

            _unitOfWorkMock.RoomRepository.Get(Arg.Any<Expression<Func<Room, bool>>>())
                .Returns(new List<Room> { expectedRoom }.AsQueryable()
                .Include(r => r.Category)); // Імітуємо Include

            var result = _roomService.GetRoom(roomId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(roomId));
            Assert.That(result.CategoryName, Is.EqualTo("Стандарт"));
            Assert.That(result.CategoryBasePrice, Is.EqualTo(1000));
        }

        [Test]
        public void UpdateRoom_ShouldUpdateExistingRoom()
        {
            var roomModel = _fixture.Build<RoomModel>()
                .With(r => r.Id, 1)
                .With(r => r.Number, "101")
                .With(r => r.CategoryId, 1)
                .With(r => r.Status, "Available")
                .Create();

            var existingRoom = _fixture.Build<Room>()
                .With(r => r.Id, 1)
                .With(r => r.Number, "101")
                .With(r => r.Status, RoomStatus.Booked)
                .Create();

            _unitOfWorkMock.RoomRepository.GetById(roomModel.Id).Returns(existingRoom);

            _roomService.UpdateRoom(roomModel);

            _unitOfWorkMock.RoomRepository.Received(1).Update(Arg.Is<Room>(r =>
                r.Id == roomModel.Id &&
                r.Number == roomModel.Number &&
                r.Status == RoomStatus.Available));
            _unitOfWorkMock.Received(1).Save();
        }

        private IMapper CreateTestMapper()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Room, RoomModel>()
                   .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name))
                   .ForMember(dest => dest.CategoryBasePrice, opt => opt.MapFrom(src => src.Category.BasePrice))
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

                cfg.CreateMap<RoomModel, Room>()
                   .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                       Enum.Parse<RoomStatus>(src.Status)));

                cfg.CreateMap<Booking, BookingModel>();
                cfg.CreateMap<BookingModel, Booking>();
            });

            return config.CreateMapper();
        }
    }
}