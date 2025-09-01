using System;
using user_panel.Data;

namespace user_panel.ViewModels
{
    public class BookingViewModel
    {
        public string userId {  get; set; }
        public string fullName { get; set; }
        public int Id { get; set; }
        // These properties will now hold the time already converted to Turkey's time zone for display.
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // --- NEW PROPERTIES ---
        // These will hold the original UTC times, purely for the status calculation logic.
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public decimal TotalPrice { get; set; }
        public string CabinLocation { get; set; }

        public string CurrentStatus
        {
            get
            {
                var now = DateTime.Now;

                if (now >= StartTime && now < EndTime)
                {
                    return "Active";
                }
                if (now >= EndTime)
                {
                    return "Completed";
                }
                return "Upcoming";
            }
        }

        public string StatusBadgeClass
        {
            get
            {
                switch (CurrentStatus)
                {
                    case "Active": return "bg-danger";
                    case "Completed": return "bg-secondary";
                    case "Upcoming": return "bg-success";
                    default: return "bg-info";
                }
            }
        }
    }
}