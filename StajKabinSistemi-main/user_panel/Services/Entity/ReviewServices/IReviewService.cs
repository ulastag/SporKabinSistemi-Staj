using user_panel.Entity;
using user_panel.Services.Base;
using user_panel.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace user_panel.Services.Entity.ReviewServices
{
    public interface IReviewService : IEntityService<Review, int>
    {
        Task<List<ReviewViewModel>> GetReviewsForCabinAsync(int cabinId);
        Task<ReviewResultViewModel> AddReviewAsync(int cabinId, string userId, string content, double rating);
        Task<bool> CanUserReviewCabinAsync(int cabinId, string userId);
    }
}