using user_panel.Data;
using System;

namespace user_panel.Entity
{
    public class Review
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public double Rating { get; set; } // 1.0, 2.5, 4.5 gibi değerleri tutabilmek için double
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- İlişkiler ---
        public int CabinId { get; set; }
        public Cabin Cabin { get; set; } = null!;

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}