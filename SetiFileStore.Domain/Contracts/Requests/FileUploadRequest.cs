using Microsoft.AspNetCore.Http;

namespace SetiFileStore.Domain.Contracts.Requests;

public class FileUploadRequest {
    public string AppDomain { get; set; }
    public string FileName { get; set; }
    public IFormFile File { get; set; }
}