namespace SetiFileStore.Domain.Contracts.Responses;

public class GetFileInfoResponse {
    public string Filename { get; set; }
    public string ContentType { get; set; }
}