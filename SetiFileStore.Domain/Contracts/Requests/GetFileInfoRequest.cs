namespace SetiFileStore.Domain.Contracts.Requests;

public class GetFileInfoRequest {
    public string FileId { get; set; }
    public string AppDomain { get; set; }   
}