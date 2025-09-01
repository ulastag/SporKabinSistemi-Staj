using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using user_panel.Context;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.WorkoutService {
    public class WorkoutService(ApplicationDbContext context, IWebHostEnvironment env) : EntityService<WorkoutPlan, int>(context), IWorkoutService {
        
        public async Task<List<WorkoutPreviewViewModel>> GetWorkoutPreviewsAsync() {
            var workouts = await _context.WorkoutPlans
               .AsNoTracking()
               .Select(p => new WorkoutPreviewViewModel {
                   WorkoutId = p.WorkoutPlanId,
                   WorkoutName = p.WorkoutName,
                   WorkoutPreview = p.WorkoutPreview,
                   WorkoutImage =p.WorkoutImage // Store just the path initially
               })
               .ToListAsync();
            return workouts;
        }



        public async Task<int> CreateWorkoutPlanAsync(string name, string preview, string details,
            List<int> selectedExerciseIds, IFormFile? workoutImage) {

            string? imagePath = null; // Varsayılan olarak null, eğer resim yüklenmezse

            try { 
            if (workoutImage is not null) {
                // 1) Doğru kök: wwwroot
                var uploadsFolder = Path.Combine(env.WebRootPath, "images", "workouts");
                Directory.CreateDirectory(uploadsFolder);

                // 2) Güvenli dosya adı + uzantı whitelist
                var original = Path.GetFileName(workoutImage.FileName); // path parçalarını kırpar
                var ext = Path.GetExtension(original).ToLowerInvariant();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                    throw new InvalidOperationException("Desteklenmeyen dosya türü.");

                var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3) Asenkron güvenli yazma
                await using var fs = new FileStream(
                    filePath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true);
                await workoutImage.CopyToAsync(fs);

                imagePath = $"/images/workouts/{uniqueFileName}";
            }
            } catch (Exception ex) {
                // Loglama işlemi burada yapılabilir
                Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                
            }
            var plan = new WorkoutPlan {
            WorkoutName = name,
            WorkoutPreview = preview,
            WorkoutDetails = details,
            WorkoutImage = imagePath // Save the image path
            };

            _context.WorkoutPlans.Add(plan);
            await _context.SaveChangesAsync();

            foreach (var exerciseId in selectedExerciseIds) {
                _context.WorkoutExercises.Add(new WorkoutExercise {
                    WorkoutPlanId = plan.WorkoutPlanId,
                    ExerciseID = exerciseId
                });
            }

            await _context.SaveChangesAsync();
            return plan.WorkoutPlanId;
        }

        public async Task<int> CreateExerciseAsync(string name, string details, IEnumerable<string> steps) {
            var exercise = new Exercise {
                ExerciseName = name,
                ExerciseDetails = details,
                HowToDos = steps.Select(s => new HowToDo {
                    HowToDoStep = s
                }).ToList()
            };

            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            return exercise.ExerciseID;
        }
        public async Task<WorkoutPlan> GetWorkoutPlanById(int? id) {
            if (id == null) return null;

            var workout = await _context.WorkoutPlans
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                        .ThenInclude(e => e.HowToDos) 
                .FirstOrDefaultAsync(w => w.WorkoutPlanId == id);

            return workout;
        }
        public async Task<WorkoutPlan> DeleteWorkoutPlanById(int id) 
        {
            var plan = await _context.WorkoutPlans
                .Include(w => w.WorkoutExercises) // ilişkili kayıtlar da gelsin
                .FirstOrDefaultAsync(w => w.WorkoutPlanId == id);

            if (plan == null)
                return null;
            //delete image if exists in file system
            if (!string.IsNullOrEmpty(plan.WorkoutImage)) {
                string fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                    plan.WorkoutImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(fullPath)) {
                    File.Delete(fullPath);
                }
            }
            // İlişkili WorkoutExercise'ları sil
            _context.WorkoutExercises.RemoveRange(plan.WorkoutExercises);

                // WorkoutPlan'ı sil
                _context.WorkoutPlans.Remove(plan);

                await _context.SaveChangesAsync();

                return plan;
        }

        public async Task<bool>UpdateWorkoutPlanByModel(WorkoutUpdateViewModel vm) {
            var workoutPlan = await _context.WorkoutPlans
                .Include(w => w.WorkoutExercises)
                .FirstOrDefaultAsync(w => w.WorkoutPlanId == vm.WorkoutId);

            if (workoutPlan == null)
                return false;
            // Process and save new image if provided
            
            if (vm.WorkoutImage is not null && vm.WorkoutImage.Length > 0) {
                // Create directory if it doesn't exist
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "workouts");
                Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.WorkoutImage;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save the file
                using (var fileStream = new FileStream(filePath, FileMode.Create)) {
                    //await vm.WorkoutImage.CopyToAsync(fileStream);
                }

                // Delete old image file if exists (optional)
                if (!string.IsNullOrEmpty(workoutPlan.WorkoutImage)) {
                    string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
                        workoutPlan.WorkoutImage.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(oldImagePath)) {
                        File.Delete(oldImagePath);
                    }
                }

                // Update image path in the database
                workoutPlan.WorkoutImage = "/images/workouts/" + uniqueFileName;
            }
            // Plan bilgilerini güncelle
            workoutPlan.WorkoutName = vm.WorkoutName;
            workoutPlan.WorkoutPreview = vm.WorkoutPreview;
            workoutPlan.WorkoutDetails = vm.WorkoutDetails;

            // Eski ilişkileri kaldır
            _context.WorkoutExercises.RemoveRange(workoutPlan.WorkoutExercises);

            // Yeni egzersizleri ekle
            workoutPlan.WorkoutExercises = vm.SelectedExerciseIds
                .Select(eId => new WorkoutExercise {
                    ExerciseID = eId,
                    WorkoutPlanId = workoutPlan.WorkoutPlanId
                }).ToList();

            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<List<ExercisePreviewViewModel>> GetAllExerciseAsync() {
            return await _context.Exercises
                .Select(e => new ExercisePreviewViewModel {
                    ExerciseId = e.ExerciseID,
                    ExerciseName = e.ExerciseName
                }).ToListAsync();
        }
        public async Task<bool> DeleteExerciseByIdAsync(int exerciseId) {
            var exercise = await _context.Exercises
                .Include(e => e.WorkoutExercises)
                .FirstOrDefaultAsync(e => e.ExerciseID == exerciseId);

            if (exercise == null)
                return false;

           
            if (exercise.WorkoutExercises != null && exercise.WorkoutExercises.Any()) {
                _context.WorkoutExercises.RemoveRange(exercise.WorkoutExercises);
            }

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> UpdateExerciseAsync(ExerciseUpdateViewModel vm) {
            var exercise = await _context.Exercises
                .Include(e => e.HowToDos)
                .FirstOrDefaultAsync(e => e.ExerciseID == vm.ExerciseId);

            if (exercise == null)
                return false;

           
            exercise.ExerciseName = vm.ExerciseName;
            exercise.ExerciseDetails = vm.ExerciseDetails;

            
            _context.HowToDos.RemoveRange(exercise.HowToDos);
            exercise.HowToDos = vm.Steps.Select(s => new HowToDo { HowToDoStep = s }).ToList();
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ExerciseUpdateViewModel?> GetExerciseUpdateViewModelByIdAsync(int exerciseId) {
            var exercise = await _context.Exercises
                 .Include(e => e.HowToDos)
                 .FirstOrDefaultAsync(e => e.ExerciseID == exerciseId);

            if (exercise == null)
                return null;

            return new ExerciseUpdateViewModel {
                ExerciseId = exercise.ExerciseID,
                ExerciseName = exercise.ExerciseName,
                ExerciseDetails = exercise.ExerciseDetails,
                Steps = exercise.HowToDos
                    .OrderBy(h => h.HowToDoStep) 
                    .Select(h => h.HowToDoStep)
                    .ToList()
            };
        }
    }



}
