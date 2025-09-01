using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.LogServices
{
    public class LogService(ApplicationDbContext context) : EntityService<LogEntry, int>(context), ILogService
    {
        public async Task<List<LogEntry>> GetLogsAsync()
        {
            return await _context.Logs
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();
        }

        public async Task<List<LogEntry>> GetFilteredLogsAsync(string filter)
        {
            return await _context.Logs
                .Where(log => log.Message != null &&
                              EF.Functions.Like(log.Message, $"%{filter}%"))
                .OrderByDescending(log => log.TimeStamp)
                .ToListAsync();
        }

        public async Task<LogFilterViewModel> GetFiltersAsync(string filter)
        {
            var filterOptions = new Dictionary<string, string>
            {
                { "All", "" },
                { "Login Logs", "logged" },
                { "Register Logs", "registered" },
                { "User Booking Logs", "booked" },
                { "User Update Logs", "is edited" },
                { "User Deletion Logs", "User deleted" },
                { "Booking Deletion Logs", "Booking deleted" },
                { "Booking Cancelation Logs", "cancelled" },
                { "Cabin Addition Logs", "New cabin" },
                { "Cabin Update Logs", "has been edited" },
                { "Cabin Deletion Logs", "Cabin deleted" },
                {"Workout Created", "Workout created" },
                {"Workout Deleted", "Workout deleted" }
            };

            string actualFilter = "";
            if (!string.IsNullOrEmpty(filter) && filterOptions.ContainsKey(filter))
                actualFilter = filterOptions[filter];

            return new LogFilterViewModel { filter = filter, actualFilter = actualFilter, filterOptions = filterOptions};
        }
    }
}
