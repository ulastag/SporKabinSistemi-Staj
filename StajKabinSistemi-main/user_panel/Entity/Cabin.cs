using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using user_panel.Entity;

namespace user_panel.Data
{
    public class Cabin
    {
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public decimal PricePerHour { get; set; }

        public int DistrictId { get; set; } 
        public District District { get; set; } = null!;
        public string QrCode { get; set; } = Guid.NewGuid().ToString();
        public string ImageUrl { get; set; }
        public double AverageRating { get; set; } = 0;
        public int TotalReviews { get; set; } = 0;
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}