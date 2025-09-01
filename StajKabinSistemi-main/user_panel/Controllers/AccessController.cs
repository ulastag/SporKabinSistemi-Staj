using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_panel.Helpers;
using user_panel.Services.Entity.ApplicationUserServices;
using user_panel.Services.Entity.BookingServices;
using user_panel.Services.Entity.CabinServices;

namespace user_panel.Controllers
{
    [Authorize]
    [ApiController] // <-- 1. DEĞİŞİKLİK: API standartlarını etkinleştir.
    [Route("api/[controller]")] // <-- 2. DEĞİŞİKLİK: Standart route tanımı. URL: /api/access
    public class AccessController : ControllerBase // <-- 1. DEĞİŞİKLİK: Controller'ı ControllerBase yap.
    {
        private readonly IBookingService _bookingService;
        private readonly IApplicationUserService _userService;
        private readonly ICabinService _cabinService;
        private readonly Serilog.ILogger _logger;

        public AccessController(
            IBookingService bookingService,
            IApplicationUserService userService,
            ICabinService cabinService,
            IConfiguration configuration)
        {
            _bookingService = bookingService;
            _userService = userService;
            _cabinService = cabinService;
            _logger = LoggerHelper.GetManualLogger(configuration);
        }

        public class UnlockRequest
        {
            public string QrCodeData { get; set; }
        }

        [HttpPost("unlock")] // <-- 2. DEĞİŞİKLİK: Metot için route tanımı. URL: POST /api/access/unlock
        public async Task<IActionResult> Unlock([FromBody] UnlockRequest request)
        {
            // --- 4. DEĞİŞİKLİK: Daha verimli şekilde sadece kullanıcı ID'sini al.
            var userId = _userService.GetCurrentUserId(User);

            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "Kullanıcı kimliği doğrulanamadı." });
            }

            if (string.IsNullOrWhiteSpace(request?.QrCodeData))
            {
                return BadRequest(new { success = false, message = "QR Kod verisi boş olamaz." });
            }

            var cabin = await _cabinService.GetCabinByQrCodeAsync(request.QrCodeData);
            if (cabin == null)
            {
                return NotFound(new { success = false, message = "Geçersiz QR Kod. Bu kabin bulunamadı." });
            }

            // --- 3. DEĞİŞİKLİK: Tamamen bu iş için yazdığımız, daha okunaklı ve verimli servis metodunu kullan.
            var validBooking = await _bookingService.GetValidBookingForCheckInAsync(userId, cabin.Id);

            if (validBooking != null)
            {
                // BAŞARILI!
                return Ok(new { success = true, message = $"Hoş geldiniz! Antrenmanınızın keyfini çıkarın." });
            }
            else
            {
                // BAŞARISIZ!
                return Unauthorized(new { success = false, message = "Bu saatte bu kabin için geçerli bir rezervasyonunuz bulunmuyor." });
            }
        }
    }
}