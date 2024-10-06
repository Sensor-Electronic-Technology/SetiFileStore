namespace Domain.Contracts.Requests;

public class MultipleFileDownloadRequest {
    public string AppDomain { get; set; }
    public string FileId { get; set; }
}