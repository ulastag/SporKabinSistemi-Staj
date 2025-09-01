using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Helpers;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;
using user_panel.Services.Entity.LogServices;
using user_panel.Services.Entity.ReviewServices;
using user_panel.Services.Firebase;
using user_panel.ViewModels;

namespace user_panel.Controllers
{
    [Authorize] // Controller'ın tamamı için giriş yapma zorunluluğu
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly ICabinService _cabinService;
        private readonly IApplicationUserService _userService;
        private readonly IFirebaseService _firebaseService;
        private readonly IReviewService _reviewService;
        private readonly ILogService _logService;
        private readonly Serilog.ILogger _logger;

        public BookingController(
            IBookingService bookingService,
            ICabinService cabinService,
            IApplicationUserService userService,
            IFirebaseService firebaseService,
            IReviewService reviewService,
            ILogService logService, 
            IConfiguration configuration)
        {
            _bookingService = bookingService;
            _cabinService = cabinService;
            _userService = userService;
            _firebaseService = firebaseService;
            _reviewService = reviewService;
            _logger = LoggerHelper.GetManualLogger(configuration);
            _logService = logService;
        }

        // --- EKSİK METOT 1: INDEX ---
        [HttpGet]
        public async Task<IActionResult> Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            List<Cabin> cabins;
            if (!string.IsNullOrEmpty(searchString))
            {
                cabins = await _cabinService.SearchAsync(searchString);
            }
            else
            {
                cabins = (await _cabinService.GetCabinsWithLocationAsync()).ToList();
            }
            return View(cabins);
        }

        // --- EKSİK METOT 2: MYBOOKINGS ---
        [HttpGet]
        public async Task<IActionResult> MyBookings()
        {
            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            var bookingsFromDb = await _bookingService.GetAllWithCabinForUserAsync(user.Id);
            var bookingViewModels = bookingsFromDb.Select(b =>
            {
                var startTimeUtc = DateTime.SpecifyKind(b.StartTime, DateTimeKind.Utc);
                var endTimeUtc = DateTime.SpecifyKind(b.EndTime, DateTimeKind.Utc);
                return new BookingViewModel
                {
                    Id = b.Id,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    StartTimeUtc = startTimeUtc,
                    EndTimeUtc = endTimeUtc,
                    TotalPrice = b.Cabin.PricePerHour,
                    CabinLocation = $"{b.Cabin.District.City.Name} / {b.Cabin.District.Name}"
                };
            }).ToList();
            return View(bookingViewModels);
        }

        // --- EKSİK METOT 3: CREATE (GET) ---
        [HttpGet]
        public async Task<IActionResult> Create(int id, DateTime? bookingDate)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id);
            if (cabin == null) return NotFound();

            var date = (bookingDate.HasValue && bookingDate.Value.Date >= DateTime.Today) ? bookingDate.Value.Date : DateTime.Today;
            var startOfLocalDay = new DateTime(date.Year, date.Month, date.Day);
            var endOfLocalDay = startOfLocalDay.AddDays(1);

            var bookingsForDate = await _bookingService.GetWhereAsync(b =>
                b.CabinId == id &&
                b.StartTime >= startOfLocalDay &&
                b.StartTime < endOfLocalDay);

            var viewModel = new CreateBookingViewModel
            {
                Cabin = cabin,
                BookingDate = date,
                BookedHours = bookingsForDate.Select(b => b.StartTime.Hour).ToList(),
                MinBookingDate = DateTime.Today.ToString("yyyy-MM-dd")
            };
            return View(viewModel);
        }

        // --- EKSİK METOT 4: CREATE (POST) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int cabinId, DateTime bookingDate, int startTimeHour)
        {
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(cabinId);
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (cabin == null || currentUser == null)
            {
                return NotFound();
            }

            var bookingStartTime = bookingDate.Date.AddHours(startTimeHour);
            var bookingEndTime = bookingStartTime.AddHours(1);

            if (bookingStartTime < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Cannot book a time slot in the past.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var bookingCost = cabin.PricePerHour;
            if (currentUser.CreditBalance < bookingCost)
            {
                TempData["ErrorMessage"] = $"Insufficient funds. Your balance is {currentUser.CreditBalance:C}, but the booking costs {bookingCost:C}.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var bookingStartTimeUtc = bookingStartTime.ToUniversalTime();
            var bookingEndTimeUtc = bookingEndTime.ToUniversalTime();

            var overlapping = await _bookingService.AnyAsync(b => b.CabinId == cabinId && bookingStartTime < b.EndTime && bookingEndTime > b.StartTime);

            if (overlapping)
            {
                TempData["ErrorMessage"] = "This time slot is already booked. Please choose another time.";
                return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
            }

            var newBooking = new Booking
            {
                ApplicationUserId = currentUser.Id,
                CabinId = cabinId,
                StartTime = bookingStartTime,
                EndTime = bookingEndTime,
                Cabin = cabin
            };

            currentUser.CreditBalance -= bookingCost;
            await _bookingService.CreateAsync(newBooking);
            await _userService.UpdateAsync(currentUser);

            try
            {
                await _firebaseService.CreateAccessPassAsync(newBooking);
                TempData["SuccessMessage"] = $"✅ Your booking for {bookingStartTime:h:00 tt} is confirmed!";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FIREBASE ERROR DETAILS: {ex.ToString()}");
                TempData["WarningMessage"] = $"Your booking is confirmed, but there was an issue creating the QR access pass. Please contact support.";
            }

            var district = cabin.District.Name;
            var city = cabin.District.City.Name;

            _logger.Information("User '{Username}' (ID: '{UserId}') has booked the cabin at {District}, {City} (ID: '{CabinId}') for '{StartTime}' - '{EndTime}'",
                currentUser.UserName, currentUser.Id, district, city, cabin.Id, bookingStartTime, bookingEndTime);

            return RedirectToAction("Create", new { id = cabinId, bookingDate = bookingDate.Date });
        }

        // --- MEVCUT METOTLARINIZ ---
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var cabin = await _cabinService.GetCabinWithLocationByIdAsync(id.Value);
            if (cabin == null) return NotFound();

            var viewModel = new CabinDetailViewModel
            {
                Cabin = cabin,
                Reviews = await _reviewService.GetReviewsForCabinAsync(id.Value),
                CanUserReview = false
            };

            if (User.Identity.IsAuthenticated)
            {
                var userId = _userService.GetCurrentUserId(User);
                viewModel.CanUserReview = await _reviewService.CanUserReviewCabinAsync(id.Value, userId);
            }

            return View(viewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int cabinId, double rating, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || rating <= 0 || rating > 5)
            {
                TempData["ReviewError"] = "Please provide a valid rating and comment.";
                return RedirectToAction("Details", new { id = cabinId });
            }

            var userId = _userService.GetCurrentUserId(User);
            var result = await _reviewService.AddReviewAsync(cabinId, userId, content, rating);

            if (result.Success)
            {
                TempData["ReviewSuccess"] = result.Message;
            }
            else
            {
                TempData["ReviewError"] = result.Message;
            }

            return RedirectToAction("Details", new { id = cabinId });
        }

        // --- EKSİK METOT 5: CHECKIN ---
        [HttpGet]
        public IActionResult CheckIn(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                TempData["ErrorMessage"] = "QR scan failed: No data was received.";
                return RedirectToAction(nameof(MyBookings));
            }
            TempData["SuccessMessage"] = $"✅ Successfully scanned code: {code}";
            return RedirectToAction(nameof(MyBookings));
        }
    }
}