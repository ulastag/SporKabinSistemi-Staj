using System.Linq.Expressions;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.BookingServices
{
    public interface IBookingService : IEntityService<Booking, int>
    {
        Task<IEnumerable<Booking>> GetAllWithCabinForUserAsync(string userId);
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        Task<bool> AnyAsync(Expression<Func<Booking, bool>> predicate);
        Task<List<Booking>> GetWhereAsync(Expression<Func<Booking, bool>> predicate);
        Task<Booking?> GetValidBookingForCheckInAsync(string userId, int cabinId);
        Task<List<BookingViewModel>> GetBookingsWithFilterAsync(string? name, string? status);
    }
}
