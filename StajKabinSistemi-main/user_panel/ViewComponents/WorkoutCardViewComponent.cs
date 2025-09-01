using Microsoft.AspNetCore.Mvc;
using user_panel.Services.Entity.WorkoutService;
using user_panel.ViewModels;
namespace user_panel.ViewComponents {
    public class WorkoutCardViewComponent : ViewComponent {
        readonly IWorkoutService _service; 
        public WorkoutCardViewComponent(IWorkoutService service) {
            _service = service; 
        }
        public async Task<IViewComponentResult> InvokeAsync() {
            List<WorkoutPreviewViewModel> vm = await _service.GetWorkoutPreviewsAsync(); 
            return View(vm);
        }
    }
}