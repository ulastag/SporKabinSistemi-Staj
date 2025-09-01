using user_panel.Entity;

public class Exercise {
    public int ExerciseID { get; set; }
    public string ExerciseName { get; set; }
    public string ExerciseDetails { get; set; }

    public ICollection<HowToDo> HowToDos { get; set; } = [];
    public ICollection<WorkoutExercise> WorkoutExercises { get; set; }
}
