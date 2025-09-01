namespace user_panel.Entity {
    public class WorkoutExercise {
        public int WorkoutPlanId { get; set; }
        public WorkoutPlan WorkoutPlan { get; set; }

        public int ExerciseID { get; set; }
        public Exercise Exercise { get; set; }
    }
}
