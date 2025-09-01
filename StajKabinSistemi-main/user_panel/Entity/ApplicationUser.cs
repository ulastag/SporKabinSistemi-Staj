using Microsoft.AspNetCore.Identity;
using user_panel.Data;
using user_panel.Entity;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public decimal CreditBalance { get; set; }
    public string? ProfilePicturePath { get; set; }

    public DateTime? DateOfBirth { get; set; } // Nullable versiyon


    public ICollection<Booking> Bookings { get; set; } = [];

    public ICollection<CabinReservation> CabinReservations { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    // Navigation property for UserWorkouts
    public ICollection<UserWorkout> UserWorkouts { get; set; } = new List<UserWorkout>();
}