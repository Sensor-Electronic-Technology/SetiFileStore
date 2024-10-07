namespace SetiFileStore.Domain.Contracts.Requests;

public class FileDownloadRequest {
    public string AppDomain { get; set; }
    public string FileId { get; set; }
}