using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using user_panel.Services.Entity.WorkoutService;
using user_panel.ViewModels;
using user_panel.Entity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Configuration;
using user_panel.Helpers;
namespace user_panel.Controllers {
    public class WorkoutController : Controller {
        
        private readonly IWorkoutService _workoutService;
        private readonly Serilog.ILogger _logger;

        public WorkoutController(IWorkoutService workoutService, IConfiguration configuration) {
            _workoutService = workoutService;
            _logger = LoggerHelper.GetManualLogger(configuration); // BookingController’daki gibi
        }


        public async Task<IActionResult> Index(int? id) {
            WorkoutPlan workoutPlan = await _workoutService.GetWorkoutPlanById(id);
            if (workoutPlan == null)
                return NotFound(); 
            return View(workoutPlan);
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id) {
            var workout = await _workoutService.GetWorkoutPlanById(id);

            if (workout == null)
                return NotFound();

            var vm = new WorkoutUpdateViewModel {
                WorkoutId = workout.WorkoutPlanId,
                WorkoutName = workout.WorkoutName,
                WorkoutPreview = workout.WorkoutPreview,
                WorkoutDetails = workout.WorkoutDetails,
                SelectedExerciseIds = workout.WorkoutExercises.Select(we => we.ExerciseID).ToList(),
                AvailableExercises = await _workoutService.GetAllExerciseAsync()
            };

            return View(vm);
        }
         
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(WorkoutUpdateViewModel vm) {
            if (!ModelState.IsValid) {
                vm.AvailableExercises = await _workoutService.GetAllExerciseAsync();
                return View(vm);
            }

            var success = await _workoutService.UpdateWorkoutPlanByModel(vm);

            if (!success) {
                TempData["Error"] = "Güncelleme başarısız oldu!";
                return RedirectToAction("Index", "Home");
            }

            TempData["Success"] = "Workout başarıyla güncellendi!";
            return RedirectToAction("Index", "Home");
            
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateExercise() {


            return View("CreateExercise");
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateExercise(ExerciseCreateViewModel vm) {
            if (!ModelState.IsValid)
                return View(vm);

            // HowToDos null gelirse boş liste olarak ayarla (güvenlik için)
            var steps = vm.HowToDoSteps ?? new List<string>();


            // Service kullanımı
            var exerciseId = await _workoutService.CreateExerciseAsync(
                vm.ExerciseName,
                vm.ExerciseDetails,
                steps
            );
            TempData["Success"] = "Exercise başarıyla oluşturuldu!";
            return RedirectToAction("CreateExercise"); 
        }
        [HttpGet]
        [Authorize (Roles ="Admin")]
        public async Task<IActionResult> Create() {
            List<ExercisePreviewViewModel> exercises = await _workoutService.GetAllExerciseAsync();

            var viewModel = new WorkoutCreateViewModel {
                Exercises = exercises,
                WorkoutImage = null
            };  

            return View(viewModel);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(WorkoutCreateViewModel vm) {
            if (!ModelState.IsValid) {
                // Reload exercises for the view if validation fails
                vm.Exercises = await _workoutService.GetAllExerciseAsync();
                return View(vm);
            }

            
            int workoutId = await _workoutService.CreateWorkoutPlanAsync(
                vm.WorkoutName,
                vm.WorkoutPreview,
                vm.WorkoutDetails,
                vm.SelectedExerciseIds ?? new List<int>(),
                vm.WorkoutImage
            );

            TempData["Success"] = "Workout plan created successfully!";
            return RedirectToAction("Create");
        }
            



        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id) {
            var deleted = await _workoutService.DeleteWorkoutPlanById(id);

            if (deleted != null) {
                _logger.Information("Workout deleted: ID = {WorkoutId}, Name = {WorkoutName}",
                    deleted.WorkoutPlanId,
                    deleted.WorkoutName);
            } else {
                _logger.Warning("Workout not found or delete failed. ID = {Id}", id);
            }

            return RedirectToAction("Index", "Home");
            
        }
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> ManageExercise() {
            var vm =  await _workoutService.GetAllExerciseAsync();
            return View(vm);
        }
        [Authorize(Roles = "Admin")]        
        [HttpPost]
        public async Task<IActionResult>DeleteExercise(int exerciseId) {
            var result = await _workoutService.DeleteExerciseByIdAsync(exerciseId);
            if(result == false) 
                TempData["StatusMessage"] = "Başarısız silme İşlemi";
            else
                TempData["StatusMessage"] = "Başarılı silme İşlemi";
            return RedirectToAction("ManageExercise", "Workout"); 
        }
        [HttpGet]
        [Authorize (Roles = "Admin")]       
        public async Task<IActionResult> UpdateExercise(int exerciseId) {
            var vm = await _workoutService.GetExerciseUpdateViewModelByIdAsync(exerciseId);
            if (vm == null)
                return RedirectToAction("ManageExercise");

            return View(vm);

        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateExercise(ExerciseUpdateViewModel vm) {
            if (!ModelState.IsValid)
                return View(vm);

            var result = await _workoutService.UpdateExerciseAsync(vm);

            if (result)
                TempData["StatusMessage"] = "Egzersiz başarıyla güncellendi.";
            else
                TempData["StatusMessage"] = "Güncelleme başarısız.";

            return RedirectToAction("ManageExercise");
        }


    }
}
