using System.Collections.Generic; // This using is needed for List<T>
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using user_panel.Context;
using user_panel.Data;
using user_panel.Helpers;
using user_panel.Services.Base;
using user_panel.ViewModels;

namespace user_panel.Services.Entity.CabinServices
{
    public class CabinService : EntityService<Cabin, int>, ICabinService
    {
        private readonly Serilog.ILogger _logger;

        public CabinService(ApplicationDbContext context, IConfiguration configuration)
            : base(context)
        {
            _logger = LoggerHelper.GetManualLogger(configuration);
        }

        public async Task<List<Cabin>> GetCabinsWithLocationAsync()
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .ToListAsync();
        }


        public async Task<Cabin?> GetCabinWithLocationByIdAsync(int cabinId)
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .FirstOrDefaultAsync(c => c.Id == cabinId);
        }

        public async Task<List<Cabin>> GetCabinsByDistrictAsync(int districtId)
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .Where(c => c.DistrictId == districtId)
                .ToListAsync();
        }

        public async Task<List<Cabin>> GetCabinsByCityAsync(int cityId)
        {
            return await _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .Where(c => c.District.CityId == cityId)
                .ToListAsync();
        }


        public async Task<List<Cabin>> SearchAsync(string searchTerm)
        {
            var query = _context.Cabins
                .Include(c => c.District)
                    .ThenInclude(d => d.City)
                .AsQueryable();

            var sanitizedSearchTerm = searchTerm?.Trim().ToLower();

            if (string.IsNullOrEmpty(sanitizedSearchTerm))
            {
                var allCabins = await GetAllAsync();
                return allCabins.ToList();
            }

            query = query.Where(c =>
                c.Description.ToLower().Contains(sanitizedSearchTerm) ||
                c.District.Name.ToLower().Contains(sanitizedSearchTerm) ||
                c.District.City.Name.ToLower().Contains(sanitizedSearchTerm));

            return await query.ToListAsync();
        }

        public async Task<Cabin?> GetCabinByQrCodeAsync(string qrCode)
        {
            return await _context.Cabins.FirstOrDefaultAsync(c => c.QrCode == qrCode);
        }

        public async Task<string> CreateCabinAsync(CabinInfoViewModel model, IFormFile? cabinImage, string? username)
        {
            try
            {
                var newCabin = new Cabin
                {
                    Description = model.Description,
                    PricePerHour = model.PricePerHour,
                    DistrictId = model.DistrictId,
                    QrCode = model.QrCode,
                    ImageUrl = "",
                    AverageRating = 0,
                    TotalReviews = 0
                };

                await CreateAsync(newCabin);

                if (cabinImage != null && cabinImage.Length > 0)
                {
                    var extension = Path.GetExtension(cabinImage.FileName);
                    var fileName = $"cabin{newCabin.Id}{extension}";
                    var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "cabins", fileName);

                    using (var stream = new FileStream(absolutePath, FileMode.Create))
                    {
                        await cabinImage.CopyToAsync(stream);
                    }

                    newCabin.ImageUrl = $"/images/cabins/{fileName}";
                    _context.Cabins.Update(newCabin);
                    await _context.SaveChangesAsync();
                }

                _logger.Information("New cabin added by {User} with description: '{Description}', price per hour: '{PricePerHour}', district id: '{DistrictId}'",
                    username, newCabin.Description, newCabin.PricePerHour, newCabin.DistrictId);

                return "Cabin added successfully!";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while adding cabin");
                return "An error occurred while adding the cabin.";
            }
        }

        public async Task<CabinInfoViewModel> GetCabinForEditingAsync(Cabin cabin)
        {
            var model = new CabinInfoViewModel
            {
                Id = cabin.Id,
                Description = cabin.Description,
                PricePerHour = cabin.PricePerHour,
                CityId = cabin.District.CityId,
                DistrictId = cabin.DistrictId,
                QrCode = cabin.QrCode
            };

            return model;
        }

        public async Task<string> EditCabinAsync(Cabin existingCabin, CabinInfoViewModel model, string? username)
        {
            string oldDescription = existingCabin.Description;
            decimal oldPrice = existingCabin.PricePerHour;
            int oldDistrictId = existingCabin.DistrictId;
            string oldQrCode = existingCabin.QrCode;

            existingCabin.Description = model.Description;
            existingCabin.PricePerHour = model.PricePerHour;
            existingCabin.DistrictId = model.DistrictId;
            existingCabin.QrCode = model.QrCode;

            await UpdateAsync(existingCabin);

            _logger.Information("Cabin with id '{existingCabin}' has been edited from '{OldDescription}, '{OldPricePerHour}', '{OldDistrictId}' '{OldQrCode}' to '{Description}', '{PricePerHour}', '{DistrictId}' '{QrCode}' by {User}",
                existingCabin.Id,
                oldDescription, oldPrice, oldDistrictId, oldQrCode,
                existingCabin.Description, existingCabin.PricePerHour, existingCabin.DistrictId, existingCabin.QrCode,
                username
                );

            return $"Cabin updated successfully!";
        }

        public async Task<string> DeleteCabinAsync(Cabin cabinToDelete, string username)
        {
            await DeleteAsync(cabinToDelete.Id);
            var cityName = cabinToDelete.District.City.Name;
            var districtName = cabinToDelete.District.Name;

            _logger.Information("Cabin deleted which is located at {District}/{City} with id {cabinId} by {User}",
                districtName, cityName, cabinToDelete.Id, username);

            return $"✅ Cabin in '{districtName}, {cityName}' was successfully deleted.";
        }
    }
}