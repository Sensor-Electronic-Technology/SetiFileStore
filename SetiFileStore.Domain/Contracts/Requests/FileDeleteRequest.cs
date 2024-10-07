namespace SetiFileStore.Domain.Contracts.Requests;

public class FileDeleteRequest {
    public string? FileId { get; set; }
    public string? AppDomain { get; set; }
}