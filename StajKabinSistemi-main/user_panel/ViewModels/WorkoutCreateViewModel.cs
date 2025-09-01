using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
namespace user_panel.ViewModels{
    public class WorkoutCreateViewModel {
        [Required]
        public string WorkoutName { get; set; }
        [Required]
        public string WorkoutPreview { get; set; }
        [Required]
        public string WorkoutDetails { get; set; }
        [DataType(DataType.Upload)]
  
        public IFormFile? WorkoutImage { get; set; } 

        // Exercise ID'leri
        public List<int> SelectedExerciseIds { get; set; } = new();
        public List<ExercisePreviewViewModel> Exercises { get; set; } = new();
    } 
}

