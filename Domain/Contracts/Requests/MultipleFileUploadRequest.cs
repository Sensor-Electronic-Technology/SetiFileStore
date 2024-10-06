using Microsoft.AspNetCore.Http;

namespace Domain.Contracts.Requests;

public class MultipleFileUploadRequest {
    public string? AppDomain { get; set; }
    public List<IFormFile> Files { get; set; } = [];
}