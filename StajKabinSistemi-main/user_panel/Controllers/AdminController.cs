using System.Configuration;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Serilog;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Helpers;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.CityServices;
using user_panel.Services.Entity.DistrictServices;
using user_panel.Services.Entity.LogServices;
using user_panel.ViewModels;


namespace user_panel.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ICityService _cityService;
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService;
        private readonly IBookingService _bookingService;
        private readonly IDistrictService _districtService;
        private readonly ILogService _logService;
        private readonly Serilog.ILogger _logger;

        public AdminController(ILogService logService, IBookingService bookingService, IApplicationUserService userService, ICabinService cabinService, IDistrictService districtService, ICityService cityService, IConfiguration configuration)
        {
            _userService = userService;
            _cabinService = cabinService;
            _bookingService = bookingService;
            _districtService = districtService;
            _cityService = cityService;
            _logService = logService;
            _logger = LoggerHelper.GetManualLogger(configuration);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddCabin()
        {
            ViewBag.Cities = await _cityService.GetCitiesForAddingAsync();
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> AddCabin(CabinInfoViewModel model, IFormFile? CabinImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = await _cityService.GetCitiesForAddingAsync();
                return View(model);
            }

            string message = await _cabinService.CreateCabinAsync(model, CabinImage, User.Identity?.Name);
            TempData["StatusMessage"] = message;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetDistrictsByCity(int id)
        {
            var districts = await _districtService.GetSelectListByCityIdAsync(id);
            return Json(districts);
        }

        [HttpGet]
        public async Task<IActionResult> GetCabinsByDistrict(int districtId)
        {
            var cabins = await _cabinService.GetCabinsByDistrictAsync(districtId);
            return PartialView("_UpdateCabinTable", cabins);
        }

        [HttpGet]
        public async Task<IActionResult> GetCabinsByCity(int cityId)
        {
            var cabins = await _cabinService.GetCabinsByCityAsync(cityId);
            return PartialView("_UpdateCabinTable", cabins);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateCabin()
        {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            ViewBag.Cities = await _cityService.GetCitiesForAddingAsync();
            return View(cabins);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCabinsForFilter()
        {
            var cabins = await _cabinService.GetCabinsWithLocationAsync();
            return PartialView("_UpdateCabinTable", cabins);
        }

        [HttpGet]
        public async Task<IActionResult> EditCabin(int id)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);
            var model = await _cabinService.GetCabinForEditingAsync(cabin);
            ViewBag.Cities = await _cityService.GetSelectListAsync();
            ViewBag.Districts = await _districtService.GetSelectListByCityIdAsync(model.CityId);
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> EditCabin(CabinInfoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Cities = await _cityService.GetSelectListAsync();
                ViewBag.Districts = await _districtService.GetSelectListByCityIdAsync(model.CityId);
                return View(model);
            }
            var existingCabin = await _cabinService.GetCabinWithLocationByIdAsync(model.Id);
            TempData["StatusMessage"] = await _cabinService.EditCabinAsync(existingCabin, model, User.Identity?.Name);
            return RedirectToAction("UpdateCabin");
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCabin(int id)
        {
            var cabinToDelete = await _cabinService.GetCabinWithLocationByIdAsync(id);
            TempData["StatusMessage"] = await _cabinService.DeleteCabinAsync(cabinToDelete, User.Identity?.Name);
            return RedirectToAction("UpdateCabin");
        }

        [HttpGet]
        public async Task<IActionResult> ManageUser(string? search)
        {
            var user = await _userService.GetAllUsersWithRolesAsync(search);
            ViewBag.SearchQuery = search;
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserPartial(string? search)
        {
            var users = await _userService.GetAllUsersWithRolesAsync(search);
            return PartialView("_ManageUserTable", users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var model = await _userService.GetUserForEditAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var oldUser = await _userService.GetUserForEditAsync(model.Id);
            var success = await _userService.UpdateUserAsync(model, oldUser, User.Identity?.Name);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update user.");
                return View(model);
            }
            TempData["StatusMessage"] = "User updated successfully.";
            return RedirectToAction("ManageUser");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var deletedUser = await _userService.GetUserForEditAsync(id);
            var success = await _userService.DeleteUserAsync(id, User.Identity?.Name);
            if (!success)
            {
                TempData["StatusMessage"] = "Failed to delete user.";
            }
            TempData["StatusMessage"] = "User deleted successfully";
            return RedirectToAction("ManageUser");
        }

        [HttpGet]
        public async Task<IActionResult> ManageBookings(string? name, string? status)
        {
            var bookings = await _bookingService.GetBookingsWithFilterAsync(name, status);
            ViewBag.Name = name;
            ViewBag.Status = status;
            return View(bookings);
        }


        [HttpPost]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            await _bookingService.DeleteAsync(id);
            TempData["StatusMessage"] = "Booking deleted successfully.";
            _logger.Information("Booking deleted with id '{bookingId}' by '{User}'",
                id, User.Identity?.Name);
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBookingAndRefund(int id, string userId, decimal credit)
        {
            await _bookingService.DeleteAsync(id);
            await _userService.AddCreditAsync(id, User.Identity?.Name, userId, credit);
            TempData["StatusMessage"] = "Booking deleted successfully.";
            return RedirectToAction("ManageBookings");
        }

        [HttpGet]
        public async Task<IActionResult> ViewLogs(string filter)
        {
            var filters = await _logService.GetFiltersAsync(filter);
            ViewBag.FilterOptions = filters.filterOptions;
            ViewBag.SelectedFilter = filters.filter;

            List<LogEntry> logs;
            if (string.IsNullOrEmpty(filter) || filter == "All")
                logs = await _logService.GetLogsAsync();
            else
                logs = await _logService.GetFilteredLogsAsync(filters.actualFilter);

            return View(logs);
        }

    }
}
