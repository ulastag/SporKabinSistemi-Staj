using user_panel.ViewModels;
using user_panel.Entity; 

namespace user_panel.Services.Entity.WorkoutService {
    public interface IWorkoutService {
        #region WorkoutPlan
        Task<List<WorkoutPreviewViewModel>> GetWorkoutPreviewsAsync();
        Task<int> CreateWorkoutPlanAsync(string name, string preview, string details, 
            List<int> selectedExerciseIds, IFormFile? workoutImage);
        Task<WorkoutPlan> GetWorkoutPlanById(int? id);
        Task<WorkoutPlan> DeleteWorkoutPlanById(int id);
        Task<bool> UpdateWorkoutPlanByModel(WorkoutUpdateViewModel vm);
        #endregion
        #region Exercise
        Task<int> CreateExerciseAsync(string name, string details, IEnumerable<string> steps);
        Task<List<ExercisePreviewViewModel>> GetAllExerciseAsync();
        Task<bool> DeleteExerciseByIdAsync(int exerciseId);
        Task<bool> UpdateExerciseAsync(ExerciseUpdateViewModel vm);
        Task<ExerciseUpdateViewModel?> GetExerciseUpdateViewModelByIdAsync(int exerciseId); 
            #endregion
        }



}
