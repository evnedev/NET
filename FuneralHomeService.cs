using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class FuneralHomeService : IFuneralHomeService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<FuneralHomeService> _logger;

        public FuneralHomeService(ApplicationDbContext dbContext, ILogger<FuneralHomeService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<FuneralHome> FindByIdAsync(int id)
        {
            return await _dbContext.FuneralHomes.FindAsync(id);
        }

        public async Task<Dictionary<string, string>> FindStateCityAsync(string state, string city)
        {
            var cities = await _dbContext.FuneralHomes
                .Distinct()
                .Select(home => new FuneralHome {City = home.City, State = home.State})
                .Where(home => home.State.ToLower() == state)
                .AsNoTracking()
                .ToListAsync();

            return cities.Where(home => home.CitySlug == city).Select(home =>
                new Dictionary<string, string> {{"City", home.City}, {"State", home.State}}).FirstOrDefault();
        }

        public async Task<IReadOnlyList<FuneralHome>> ListAllAsync()
        {
            return await _dbContext.FuneralHomes.AsNoTracking().ToListAsync();
        }

        public async Task<bool> CreateAsync(FuneralHome home)
        {
            _dbContext.FuneralHomes.Add(home);
            var result = await _dbContext.SaveChangesAsync();

            return result == 1;
        }

        public async Task<bool> UpdateAsync(FuneralHome home)
        {
            _dbContext.Attach(home).State = EntityState.Modified;

            try
            {
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Funeral Home successfully updated.");

                return true;
            }
            catch (DbUpdateConcurrencyException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(FuneralHome home)
        {
            try
            {
                _dbContext.FuneralHomes.Remove(home);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Funeral Home successfully deleted.");

                return true;
            }
            catch (DbUpdateException e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
    }
}