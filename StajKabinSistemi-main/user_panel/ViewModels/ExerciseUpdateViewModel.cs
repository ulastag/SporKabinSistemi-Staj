using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels {
    public class ExerciseUpdateViewModel {
        public int ExerciseId { get; set; }

        [Required]
        public string ExerciseName { get; set; }

        public string ExerciseDetails { get; set; }

        [MinLength(1, ErrorMessage = "En az bir adım girilmelidir")]
        public List<string> Steps { get; set; } = new();
    }
}
