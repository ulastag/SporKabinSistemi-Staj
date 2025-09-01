using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.BookingServices
{
    public class BookingService(ApplicationDbContext context) : EntityService<Booking, int>(context), IBookingService
    {
        public async Task<IEnumerable<Booking>> GetAllWithCabinForUserAsync(string userId)
        {
            return await _dbSet
                .Where(b => b.ApplicationUserId == userId)
                .Include(b => b.Cabin)
                    .ThenInclude(c => c.District)
                    .ThenInclude(d => d.City)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
        {
            return await _dbSet
                .Include(b => b.ApplicationUser)
                .Include(b => b.Cabin)
                    .ThenInclude(c => c.District)
                                        .ThenInclude(d => d.City)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<Booking, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }
        public async Task<List<Booking>> GetWhereAsync(Expression<Func<Booking, bool>> predicate)
        {
            return await _context.Bookings
                .Where(predicate) // Apply the dynamic WHERE clause
                .ToListAsync();
        }
        public async Task<Booking?> GetValidBookingForCheckInAsync(string userId, int cabinId)
        {
            var currentTime = DateTime.UtcNow;

            return await _context.Bookings
                .Where(b => b.ApplicationUserId == userId
                         && b.CabinId == cabinId
                         && b.StartTime <= currentTime
                         && b.EndTime > currentTime)
                .FirstOrDefaultAsync();

        }

        public async Task<List<BookingViewModel>> GetBookingsWithFilterAsync(string? name, string? status)
        {
            var bookings = await _context.Bookings
                .Include(b => b.ApplicationUser)
                .Include(b => b.Cabin)
                    .ThenInclude(c => c.District)
                        .ThenInclude(d => d.City)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim().ToLower();
                bookings = bookings
                    .Where(b => ($"{b.ApplicationUser.FirstName} {b.ApplicationUser.LastName}").ToLower().Contains(name))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (status == "upcoming")
                    bookings = bookings.Where(b => b.StartTime > DateTime.Now).ToList();
                else if (status == "active")
                    bookings = bookings.Where(b => b.StartTime <= DateTime.Now && b.EndTime > DateTime.Now).ToList();
                else if (status == "completed")
                    bookings = bookings.Where(b => b.StartTime <= DateTime.Now).ToList();
            }

            var viewModel = bookings.Select(b => new BookingViewModel
            {
                userId = b.ApplicationUser.Id,
                fullName = $"{b.ApplicationUser.FirstName} {b.ApplicationUser.LastName}",
                Id = b.Id,
                StartTimeUtc = b.StartTime,
                EndTimeUtc = b.EndTime,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                TotalPrice = b.Cabin.PricePerHour,
                CabinLocation = $"📍 {b.Cabin.District.City.Name} / {b.Cabin.District.Name}"
            }).ToList();

            return viewModel;
        }

    }
}
