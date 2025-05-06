using AutoMapper;
using HotelApp.Core.Factories;
using HotelApp.Core.Interfaces;
using HotelApp.Core.Models;
using HotelApp.DAL.Entities;
using HotelApp.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelApp.Core.Services
{
    public class RoomService : IRoomService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoomService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public IEnumerable<RoomModel> GetAvailableRooms(DateTime date)
        {
            var rooms = _unitOfWork.RoomRepository.Get(
                r => r.Status == RoomStatus.Available &&
                    (!r.Bookings.Any() || !r.Bookings.Any(b => b.StartDate <= date && b.EndDate > date)))
                .Include(r => r.Category)
                .ToList();

            return _mapper.Map<IEnumerable<RoomModel>>(rooms);
        }

        public (RoomModel room, decimal totalPrice, BookingModel bookingPreview) PreviewBooking(int roomId, DateTime from, DateTime to, string clientName)
        {
            var room = _unitOfWork.RoomRepository.Get(r => r.Id == roomId)
                .Include(r => r.Category)
                .Include(r => r.Bookings)
                .FirstOrDefault();

            if (room == null || room.Status != RoomStatus.Available ||
                room.Bookings.Any(b => b.StartDate < to && b.EndDate > from))
                return (null, 0, null);

            int days = (to - from).Days;
            decimal total = days * room.Category.BasePrice;

            var bookingModel = new BookingModel
            {
                RoomId = roomId,
                StartDate = from,
                EndDate = to,
                ClientName = clientName,
                TotalPrice = total
            };

            return (_mapper.Map<RoomModel>(room), total, bookingModel);
        }

        public void ConfirmBooking(BookingModel bookingModel)
        {
            var booking = _mapper.Map<Booking>(bookingModel);
            _unitOfWork.BookingRepository.Add(booking);

            var room = _unitOfWork.RoomRepository.GetById(bookingModel.RoomId);
            room.Status = RoomStatus.Booked;
            _unitOfWork.RoomRepository.Update(room);

            _unitOfWork.Save();
        }

        public void CancelBooking(int roomId)
        {
            var room = _unitOfWork.RoomRepository.Get(r => r.Id == roomId)
                .Include(r => r.Bookings)
                .FirstOrDefault();

            if (room == null) return;

            var bookings = room.Bookings.ToList();
            foreach (var booking in bookings)
            {
                _unitOfWork.BookingRepository.Delete(booking);
            }

            room.Status = RoomStatus.Available;
            _unitOfWork.RoomRepository.Update(room);
            _unitOfWork.Save();
        }

        public void SeedData()
        {
            if (!_unitOfWork.CategoryRepository.GetAll().Any())
            {
                var standard = new RoomCategory { Name = "Стандарт", BasePrice = 1000 };
                var deluxe = new RoomCategory { Name = "Делюкс", BasePrice = 2000 };

                _unitOfWork.CategoryRepository.Add(standard);
                _unitOfWork.CategoryRepository.Add(deluxe);
                _unitOfWork.Save();

                _unitOfWork.RoomRepository.Add(new Room { Number = "101", CategoryId = standard.Id, Status = RoomStatus.Available });
                _unitOfWork.RoomRepository.Add(new Room { Number = "102", CategoryId = standard.Id, Status = RoomStatus.Available });
                _unitOfWork.RoomRepository.Add(new Room { Number = "201", CategoryId = deluxe.Id, Status = RoomStatus.Available });
                _unitOfWork.Save();
            }
        }

        public IEnumerable<RoomModel> GetAllRooms()
        {
            var rooms = _unitOfWork.RoomRepository.GetAll()
                .Include(r => r.Category)
                .ToList();
            return _mapper.Map<IEnumerable<RoomModel>>(rooms);
        }

        public RoomModel GetRoom(int id)
        {
            var room = _unitOfWork.RoomRepository.Get(r => r.Id == id)
                .Include(r => r.Category)
                .FirstOrDefault();
            return _mapper.Map<RoomModel>(room);
        }

        public void AddRoomViaFactory(IRoomFactory factory, string number)
        {
            var (room, category) = factory.CreateRoom(number);

            var existingCategory = _unitOfWork.CategoryRepository.Get(
                c => c.Name == category.Name).FirstOrDefault();

            if (existingCategory != null)
            {
                room.Category = existingCategory;
            }
            else
            {
                _unitOfWork.CategoryRepository.Add(category);
            }

            _unitOfWork.RoomRepository.Add(room);
            _unitOfWork.Save();
        }

        public void UpdateRoom(RoomModel roomModel)
        {
            var room = _mapper.Map<Room>(roomModel);
            _unitOfWork.RoomRepository.Update(room);
            _unitOfWork.Save();
        }

        public void DeleteRoom(int id)
        {
            _unitOfWork.RoomRepository.Delete(id);
            _unitOfWork.Save();
        }
    }
}