using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TravelNest.Application.DTOs.Common;
using TravelNest.Application.DTOs.Properties;
using TravelNest.Application.Interfaces;
using TravelNest.Domain.Entities;
using TravelNest.Domain.Enums;
using TravelNest.Domain.Interfaces;

namespace TravelNest.Infrastructure.Services;

public class PropertyService : IPropertyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IFileStorageService _fileStorage;

    public PropertyService(IUnitOfWork unitOfWork, IMapper mapper, IFileStorageService fileStorage)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _fileStorage = fileStorage;
    }

    public async Task<ApiResponse<PropertyDto>> CreateAsync(Guid providerId, CreatePropertyDto dto)
    {
        var provider = await _unitOfWork.Users.GetByIdAsync(providerId);
        if (provider == null || provider.Role != UserRole.Provider)
            return ApiResponse<PropertyDto>.FailResponse("Invalid provider");

        var property = _mapper.Map<Property>(dto);
        property.ProviderId = providerId;

        await _unitOfWork.Properties.AddAsync(property);

        if (dto.Amenities?.Any() == true)
        {
            foreach (var amenity in dto.Amenities)
            {
                await _unitOfWork.PropertyAmenities.AddAsync(new PropertyAmenity
                {
                    Name = amenity,
                    PropertyId = property.Id
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PropertyDto>.SuccessResponse(
            await GetPropertyDtoAsync(property.Id),
            "Property created successfully. Pending admin approval.");
    }

    public async Task<ApiResponse<PropertyDto>> UpdateAsync(Guid providerId, Guid propertyId, UpdatePropertyDto dto)
    {
        var property = await _unitOfWork.Properties.Query()
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.ProviderId == providerId);

        if (property == null)
            return ApiResponse<PropertyDto>.FailResponse("Property not found");

        _mapper.Map(dto, property);

        if (dto.Amenities != null)
        {
            // Remove old amenities
            var oldAmenities = property.Amenities.ToList();
            _unitOfWork.PropertyAmenities.RemoveRange(oldAmenities);

            // Add new
            foreach (var amenity in dto.Amenities)
            {
                await _unitOfWork.PropertyAmenities.AddAsync(new PropertyAmenity
                {
                    Name = amenity,
                    PropertyId = property.Id
                });
            }
        }

        _unitOfWork.Properties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<PropertyDto>.SuccessResponse(
            await GetPropertyDtoAsync(property.Id), "Property updated");
    }

    public async Task<ApiResponse<bool>> DeleteAsync(Guid providerId, Guid propertyId)
    {
        var property = await _unitOfWork.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.ProviderId == providerId);

        if (property == null)
            return ApiResponse<bool>.FailResponse("Property not found");

        property.IsDeleted = true;
        _unitOfWork.Properties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Property deleted");
    }

    public async Task<ApiResponse<PropertyDto>> GetByIdAsync(Guid propertyId, Guid? currentUserId = null)
    {
        var dto = await GetPropertyDtoAsync(propertyId, currentUserId);
        if (dto == null)
            return ApiResponse<PropertyDto>.FailResponse("Property not found");

        return ApiResponse<PropertyDto>.SuccessResponse(dto);
    }

    public async Task<ApiResponse<PagedResponse<PropertyDto>>> SearchAsync(
        PropertySearchDto search, Guid? currentUserId = null)
    {
        var query = _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .Include(p => p.Media)
            .Include(p => p.Amenities)
            .Where(p => p.IsApproved && p.IsAvailable)
            .AsQueryable();

        // Filters
        if (search.Type.HasValue)
            query = query.Where(p => p.Type == search.Type.Value);

        if (!string.IsNullOrEmpty(search.Country))
            query = query.Where(p => p.Country.ToLower().Contains(search.Country.ToLower()));

        if (!string.IsNullOrEmpty(search.City))
            query = query.Where(p => p.City.ToLower().Contains(search.City.ToLower()));

        if (search.MinPrice.HasValue)
            query = query.Where(p => p.PricePerNight >= search.MinPrice.Value);

        if (search.MaxPrice.HasValue)
            query = query.Where(p => p.PricePerNight <= search.MaxPrice.Value);

        if (search.MinGuests.HasValue)
            query = query.Where(p => p.MaxGuests >= search.MinGuests.Value);

        if (search.MinRating.HasValue)
            query = query.Where(p => p.AverageRating >= search.MinRating.Value);

        if (!string.IsNullOrEmpty(search.SearchTerm))
        {
            var term = search.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Description.ToLower().Contains(term) ||
                p.City.ToLower().Contains(term) ||
                p.Country.ToLower().Contains(term));
        }

        // Check availability
        if (search.CheckIn.HasValue && search.CheckOut.HasValue)
        {
            var bookedPropertyIds = await _unitOfWork.Bookings.Query()
                .Where(b => b.Status != BookingStatus.Cancelled &&
                           b.Status != BookingStatus.Rejected &&
                           b.CheckInDate < search.CheckOut.Value &&
                           b.CheckOutDate > search.CheckIn.Value)
                .Select(b => b.PropertyId)
                .Distinct()
                .ToListAsync();

            query = query.Where(p => !bookedPropertyIds.Contains(p.Id));
        }

        // Sort
        query = search.SortBy?.ToLower() switch
        {
            "price" => search.SortDescending
                ? query.OrderByDescending(p => p.PricePerNight)
                : query.OrderBy(p => p.PricePerNight),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();

        var properties = await query
            .Skip((search.PageNumber - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync();

        var favoriteIds = new HashSet<Guid>();
        if (currentUserId.HasValue)
        {
            favoriteIds = (await _unitOfWork.Favorites
                .FindAsync(f => f.UserId == currentUserId.Value))
                .Select(f => f.PropertyId)
                .ToHashSet();
        }

        var dtos = properties.Select(p =>
        {
            var dto = _mapper.Map<PropertyDto>(p);
            dto.IsFavorited = favoriteIds.Contains(p.Id);
            return dto;
        });

        return ApiResponse<PagedResponse<PropertyDto>>.SuccessResponse(new PagedResponse<PropertyDto>
        {
            Data = dtos,
            PageNumber = search.PageNumber,
            PageSize = search.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<PagedResponse<PropertyDto>>> GetByProviderAsync(
        Guid providerId, PagedRequest request)
    {
        var query = _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .Include(p => p.Media)
            .Include(p => p.Amenities)
            .Where(p => p.ProviderId == providerId);

        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync();
        var properties = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<PropertyDto>>.SuccessResponse(new PagedResponse<PropertyDto>
        {
            Data = _mapper.Map<IEnumerable<PropertyDto>>(properties),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<List<PropertyMediaDto>>> UploadMediaAsync(
        Guid providerId, Guid propertyId, List<IFormFile> files, MediaType mediaType)
    {
        var property = await _unitOfWork.Properties
            .FirstOrDefaultAsync(p => p.Id == propertyId && p.ProviderId == providerId);

        if (property == null)
            return ApiResponse<List<PropertyMediaDto>>.FailResponse("Property not found");

        var mediaList = new List<PropertyMedia>();

        foreach (var file in files)
        {
            var url = await _fileStorage.UploadFileAsync(file, $"properties/{propertyId}");
            var thumbnail = mediaType != MediaType.Photo
                ? await _fileStorage.GenerateThumbnailAsync(url)
                : url;

            var media = new PropertyMedia
            {
                Url = url,
                ThumbnailUrl = thumbnail,
                Type = mediaType,
                PropertyId = propertyId,
                IsCover = !await _unitOfWork.PropertyMedia.AnyAsync(m => m.PropertyId == propertyId)
            };

            await _unitOfWork.PropertyMedia.AddAsync(media);
            mediaList.Add(media);
        }

        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<List<PropertyMediaDto>>.SuccessResponse(
            _mapper.Map<List<PropertyMediaDto>>(mediaList), "Media uploaded");
    }

    public async Task<ApiResponse<bool>> DeleteMediaAsync(Guid providerId, Guid mediaId)
    {
        var media = await _unitOfWork.PropertyMedia.Query()
            .Include(m => m.Property)
            .FirstOrDefaultAsync(m => m.Id == mediaId && m.Property.ProviderId == providerId);

        if (media == null)
            return ApiResponse<bool>.FailResponse("Media not found");

        await _fileStorage.DeleteFileAsync(media.Url);
        media.IsDeleted = true;
        _unitOfWork.PropertyMedia.Update(media);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Media deleted");
    }

    public async Task<ApiResponse<bool>> ApprovePropertyAsync(Guid propertyId)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(propertyId);
        if (property == null)
            return ApiResponse<bool>.FailResponse("Property not found");

        property.IsApproved = true;
        _unitOfWork.Properties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Property approved");
    }

    public async Task<ApiResponse<bool>> RejectPropertyAsync(Guid propertyId)
    {
        var property = await _unitOfWork.Properties.GetByIdAsync(propertyId);
        if (property == null)
            return ApiResponse<bool>.FailResponse("Property not found");

        property.IsApproved = false;
        property.IsAvailable = false;
        _unitOfWork.Properties.Update(property);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Property rejected");
    }

    public async Task<ApiResponse<bool>> ToggleFavoriteAsync(Guid userId, Guid propertyId)
    {
        var existing = await _unitOfWork.Favorites
            .FirstOrDefaultAsync(f => f.UserId == userId && f.PropertyId == propertyId);

        if (existing != null)
        {
            _unitOfWork.Favorites.Remove(existing);
            await _unitOfWork.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(false, "Removed from favorites");
        }

        await _unitOfWork.Favorites.AddAsync(new Favorite
        {
            UserId = userId,
            PropertyId = propertyId
        });
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Added to favorites");
    }

    public async Task<ApiResponse<PagedResponse<PropertyDto>>> GetFavoritesAsync(
        Guid userId, PagedRequest request)
    {
        var favoritePropertyIds = await _unitOfWork.Favorites.Query()
            .Where(f => f.UserId == userId)
            .Select(f => f.PropertyId)
            .ToListAsync();

        var query = _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .Include(p => p.Media)
            .Include(p => p.Amenities)
            .Where(p => favoritePropertyIds.Contains(p.Id));

        var totalCount = await query.CountAsync();
        var properties = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        var dtos = properties.Select(p =>
        {
            var dto = _mapper.Map<PropertyDto>(p);
            dto.IsFavorited = true;
            return dto;
        });

        return ApiResponse<PagedResponse<PropertyDto>>.SuccessResponse(new PagedResponse<PropertyDto>
        {
            Data = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    public async Task<ApiResponse<PagedResponse<PropertyDto>>> GetAllPendingAsync(PagedRequest request)
    {
        var query = _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .Include(p => p.Media)
            .Include(p => p.Amenities)
            .Where(p => !p.IsApproved);

        var totalCount = await query.CountAsync();
        var properties = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return ApiResponse<PagedResponse<PropertyDto>>.SuccessResponse(new PagedResponse<PropertyDto>
        {
            Data = _mapper.Map<IEnumerable<PropertyDto>>(properties),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        });
    }

    private async Task<PropertyDto?> GetPropertyDtoAsync(Guid propertyId, Guid? currentUserId = null)
    {
        var property = await _unitOfWork.Properties.Query()
            .Include(p => p.Provider)
            .Include(p => p.Media)
            .Include(p => p.Amenities)
            .FirstOrDefaultAsync(p => p.Id == propertyId);

        if (property == null) return null;

        var dto = _mapper.Map<PropertyDto>(property);

        if (currentUserId.HasValue)
        {
            dto.IsFavorited = await _unitOfWork.Favorites
                .AnyAsync(f => f.UserId == currentUserId.Value && f.PropertyId == propertyId);
        }

        return dto;
    }
}
