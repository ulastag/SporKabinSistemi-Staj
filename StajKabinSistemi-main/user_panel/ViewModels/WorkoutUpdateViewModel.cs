using System.ComponentModel.DataAnnotations;
namespace user_panel.ViewModels {
    public class WorkoutUpdateViewModel {
        [Required]
        public int WorkoutId { get; set; } // Güncellenecek planın ID'si

        [Required(ErrorMessage = "Plan adı boş bırakılamaz")]
        [StringLength(100)]
        [Display(Name = "Workout Name")]
        public string WorkoutName { get; set; }

        [Display(Name = "Preview (Kısa Açıklama)")]
        [MaxLength(200)]
        public string WorkoutPreview { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Detaylı Açıklama")]
        public string WorkoutDetails { get; set; }

        [Display(Name = "Egzersizler")]
        [MinLength(1, ErrorMessage = "En az bir egzersiz seçmelisin")]
        public List<int> SelectedExerciseIds { get; set; } = new();
        public string? WorkoutImage { get; set; }
        public IFormFile? WorkoutUpdatedImage { get; set; }
        public List<ExercisePreviewViewModel> AvailableExercises { get; set; } = new();
    }
}

