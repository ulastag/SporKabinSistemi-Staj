namespace user_panel.Entity {
    public class UserWorkout {
        public string UserId{ get; set; } 
        public int WorkoutPlanId { get; set; }  
        public  ApplicationUser User { get; set; } = null!;
        public WorkoutPlan WorkoutPlan { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true; 
    }
}
