using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using user_panel.Context;
using user_panel.Data;
using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.Services.Entity.CabinServices;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.ReviewServices
{
    public class ReviewService : EntityService<Review, int>, IReviewService
    {
        private readonly ICabinService _cabinService;

        public ReviewService(ApplicationDbContext context, ICabinService cabinService) : base(context)
        {
            _cabinService = cabinService;
        }

        public async Task<List<ReviewViewModel>> GetReviewsForCabinAsync(int cabinId)
        {
            return await _context.Set<Review>()
                .Where(r => r.CabinId == cabinId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewViewModel
                {
                    Content = r.Content,
                    Rating = r.Rating,
                    ReviewDate = r.CreatedAt,
                    UserName = r.ApplicationUser.FirstName + " " + r.ApplicationUser.LastName.Substring(0, 1) + "."
                })
                .ToListAsync();
        }

        public async Task<ReviewResultViewModel> AddReviewAsync(int cabinId, string userId, string content, double rating)
        {
            // 1. Kullanıcının bu kabini daha önce kiralayıp kiralamadığını kontrol et
            if (!await CanUserReviewCabinAsync(cabinId, userId))
            {
                return new ReviewResultViewModel { Success = false, Message = "You can only review cabins that you have previously booked and used." };
            }

            // 2. Kullanıcının bu kabine daha önce yorum yapıp yapmadığını kontrol et
            var existingReview = await _context.Set<Review>()
                .FirstOrDefaultAsync(r => r.CabinId == cabinId && r.ApplicationUserId == userId);

            if (existingReview != null)
            {
                return new ReviewResultViewModel { Success = false, Message = "You have already submitted a review for this cabin." };
            }

            // 3. Yeni yorumu oluştur ve veritabanına ekle
            var newReview = new Review
            {
                CabinId = cabinId,
                ApplicationUserId = userId,
                Content = content,
                Rating = rating
            };

            await CreateAsync(newReview); // base class'taki CreateAsync'i kullanıyoruz

            // 4. Kabinin ortalama puanını ve yorum sayısını güncelle
            var cabin = await _cabinService.GetByIdAsync(cabinId);
            if (cabin != null)
            {
                var oldTotalRating = cabin.AverageRating * cabin.TotalReviews;
                cabin.TotalReviews++;
                cabin.AverageRating = (oldTotalRating + newReview.Rating) / cabin.TotalReviews;
                await _cabinService.UpdateAsync(cabin);
            }

            return new ReviewResultViewModel { Success = true, Message = "Your review has been successfully submitted. Thank you!" };
        }

        public async Task<bool> CanUserReviewCabinAsync(int cabinId, string userId)
        {
            // Kullanıcının bu kabin için geçmişte tamamlanmış bir rezervasyonu var mı?
            return await _context.Set<Booking>()
                .AnyAsync(b => b.CabinId == cabinId &&
                               b.ApplicationUserId == userId &&
                               b.EndTime < DateTime.UtcNow);
        }
    }
}