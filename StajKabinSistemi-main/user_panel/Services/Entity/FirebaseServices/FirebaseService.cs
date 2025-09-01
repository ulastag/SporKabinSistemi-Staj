using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using user_panel.Data;
using System.Threading.Tasks;

namespace user_panel.Services.Firebase
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly ILogger<FirebaseService> _logger;

        public FirebaseService(FirestoreDb firestoreDb, ILogger<FirebaseService> logger)
        {
            _firestoreDb = firestoreDb;
            _logger = logger;
        }

        public async Task CreateAccessPassAsync(Booking booking)
        {
            if (booking == null)
            {
                _logger.LogWarning("CreateAccessPassAsync: 'booking' nesnesi null geldi.");
                return;
            }
            if (string.IsNullOrEmpty(booking.ApplicationUserId))
            {
                _logger.LogWarning("CreateAccessPassAsync: Booking nesnesi ApplicationUserId içermiyor. Booking ID: {BookingId}", booking.Id);
                return;
            }

            var accessPassData = new
            {
                userId = booking.ApplicationUserId,
                gymId = booking.Cabin?.QrCode, // Kabin null olabilir diye '?.' eklemek daha güvenli
                startTime = DateTime.SpecifyKind(booking.StartTime, DateTimeKind.Utc),
                endTime = DateTime.SpecifyKind(booking.EndTime, DateTimeKind.Utc)
            };

            // --- DEĞİŞİKLİK BURADA ---
            // Belge ID'sini "BookingId-ApplicationUserId" formatında oluşturuyoruz.
            string documentId = $"{booking.Id}-{booking.ApplicationUserId}";
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            _logger.LogInformation("Firestore'a belge oluşturuluyor: {DocumentId}", documentId);
            await docRef.SetAsync(accessPassData);
            _logger.LogInformation("Firestore belgesi başarıyla oluşturuldu: {DocumentId}", documentId);
        }

        public async Task DeleteAccessPassAsync(Booking booking)
        {
            if (booking == null)
            {
                _logger.LogWarning("DeleteAccessPassAsync: 'booking' nesnesi null geldi.");
                return;
            }
            if (string.IsNullOrEmpty(booking.ApplicationUserId))
            {
                _logger.LogWarning("DeleteAccessPassAsync: Booking nesnesi ApplicationUserId içermiyor. Booking ID: {BookingId}", booking.Id);
                return;
            }

            // --- DEĞİŞİKLİK BURADA ---
            // Silinecek belgenin ID'sini de aynı formatla oluşturuyoruz ki doğru belgeyi bulalım.
            string documentId = $"{booking.Id}-{booking.ApplicationUserId}";
            DocumentReference docRef = _firestoreDb.Collection("active_reservations").Document(documentId);

            _logger.LogInformation("Firestore'dan belge siliniyor: {DocumentId}", documentId);
            await docRef.DeleteAsync();
            _logger.LogInformation("Firestore belgesi başarıyla silindi: {DocumentId}", documentId);
        }
    }
}