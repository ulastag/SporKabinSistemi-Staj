using Microsoft.EntityFrameworkCore;
using Serilog;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.ConstrainedExecution;

namespace user_panel.Entity {
    public class WorkoutPlan {
        
        public int WorkoutPlanId { get; set;}
        public string WorkoutName { get; set;}
        public string WorkoutPreview { get; set; }
        public string WorkoutDetails { get; set; }
        public string? WorkoutImage { get; set; }

        public ICollection<WorkoutExercise> WorkoutExercises { get; set; }
        public ICollection<UserWorkout> UserWorkouts { get; set; } = new List<UserWorkout>();

        [NotMapped]
        public IEnumerable<Exercise> Exercises => WorkoutExercises?.Select(we => we.Exercise);
    }
}
