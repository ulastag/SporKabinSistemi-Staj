using System.ComponentModel.DataAnnotations;

namespace user_panel.ViewModels {
    public class ExerciseCreateViewModel {
        [Required]
        [MaxLength(100)]
        public string ExerciseName { get; set; }

        [MaxLength(1000)]
        public string ExerciseDetails { get; set; }

        // Kullanıcıdan birden fazla adım alınacak
        public List<string> HowToDoSteps { get; set; } = new();
    }

}
