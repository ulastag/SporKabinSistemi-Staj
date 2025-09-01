using System.Collections.Generic; // <-- ADD THIS for List<T>
using System.Threading.Tasks;   // <-- ADD THIS for Task<>
using Microsoft.AspNetCore.Mvc.Rendering;
using user_panel.Data;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.CabinServices
{
    public interface ICabinService : IEntityService<Cabin, int>
    {
        Task<List<Cabin>> GetCabinsWithLocationAsync();
        Task<Cabin?> GetCabinWithLocationByIdAsync(int cabinId);
        Task<List<Cabin>> GetCabinsByDistrictAsync(int districtId);
        Task<List<Cabin>> GetCabinsByCityAsync(int cityId);
        Task<List<Cabin>> SearchAsync(string searchTerm);
        Task<Cabin?> GetCabinByQrCodeAsync(string qrCode);
        Task<string> CreateCabinAsync(CabinInfoViewModel model, IFormFile? cabinImage, string? username);
        Task<CabinInfoViewModel> GetCabinForEditingAsync(Cabin cabin);
        Task<string> EditCabinAsync(Cabin existingCabin, CabinInfoViewModel model, string? username);
        Task<string> DeleteCabinAsync(Cabin cabin, string username);
    }
}