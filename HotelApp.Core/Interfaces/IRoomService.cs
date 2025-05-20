using HotelApp.Core.Factories;
using HotelApp.Core.Models;
using System;
using System.Collections.Generic;

namespace HotelApp.Core.Interfaces
{
    public interface IRoomService
    {
        IEnumerable<RoomModel> GetAvailableRooms(DateTime date);
        (RoomModel room, decimal totalPrice, BookingModel bookingPreview) PreviewBooking(int roomId, DateTime from, DateTime to, string clientName);
        void ConfirmBooking(BookingModel booking);
        void CancelBooking(int roomId);
        IEnumerable<RoomModel> GetAllRooms();
        RoomModel GetRoom(int id);
        void AddRoomViaFactory(string factoryType, string number);
        void UpdateRoom(RoomModel roomModel);
        void DeleteRoom(int id);
        void SeedData();
    }
}