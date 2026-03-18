
using Microsoft.AspNetCore.Http;

namespace TravelNest.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folder);
    Task<bool> DeleteFileAsync(string fileUrl);
    Task<string> GenerateThumbnailAsync(string fileUrl);
}