using Microsoft.EntityFrameworkCore;
using AssetFlow.Data.Data;
using AssetFlow.Data.Entities.Tenant;
using AssetFlow.Services.Contracts;
using AssetFlow.Services.Dto;
using AssetFlow.Services.Dto.Tenant;
using System.Net;

namespace AssetFlow.Services.Core
{
    public class LanguageService : ILanguageService
    {
        private readonly ApplicationDbContext _context;

        public LanguageService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseDto<LanguageDto>> CreateAsync(CreateLanguageDto dto)
        {
            var response = new ResponseDto<LanguageDto>();

            if (await _context.Languages.AnyAsync(x => x.Code == dto.Code))
            {
                response.AddError("Language code already exists.");
                response.StatusCode = HttpStatusCode.Conflict;
                return response;
            }

            var entity = new Language
            {
                Name = dto.Name,
                DisplayName = dto.DisplayName,
                Order = dto.Order,
                Code = dto.Code,
                IsActive = true
            };

            _context.Languages.Add(entity);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<LanguageDto>> UpdateAsync(UpdateLanguageDto dto)
        {
            var response = new ResponseDto<LanguageDto>();

            var entity = await _context.Languages.FindAsync(dto.Id);
            if (entity == null)
            {
                response.AddError("Language not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.Name = dto.Name;
            entity.DisplayName = dto.DisplayName;
            entity.Order = dto.Order;
            entity.Code = dto.Code;

            _context.Languages.Update(entity);
            await _context.SaveChangesAsync();

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<LanguageDto>> GetByIdAsync(int id)
        {
            var response = new ResponseDto<LanguageDto>();

            var entity = await _context.Languages.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Language not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            response.Result = MapToDto(entity);
            return response;
        }

        public async Task<ResponseDto<List<LanguageDto>>> GetAllAsync()
        {
            var response = new ResponseDto<List<LanguageDto>>();

            var languages = await _context.Languages
                .OrderBy(x => x.Order)
                .Select(x => MapToDto(x))
                .ToListAsync();

            response.Result = languages;
            return response;
        }

        public async Task<ResponseDto<bool>> ToggleStatusAsync(int id)
        {
            var response = new ResponseDto<bool>();

            var entity = await _context.Languages.FindAsync(id);
            if (entity == null)
            {
                response.AddError("Language not found.");
                response.StatusCode = HttpStatusCode.NotFound;
                return response;
            }

            entity.IsActive = !entity.IsActive;
            await _context.SaveChangesAsync();

            response.Result = entity.IsActive;
            return response;
        }

        private static LanguageDto MapToDto(Language entity)
        {
            return new LanguageDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayName = entity.DisplayName,
                Order = entity.Order,
                Code = entity.Code,
                IsActive = entity.IsActive
            };
        }
    }
}
