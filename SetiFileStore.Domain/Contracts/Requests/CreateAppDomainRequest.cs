namespace SetiFileStore.Domain.Contracts.Requests;

public class CreateAppDomainRequest {
    public string? AppDomain { get; set; }
    public string? Description { get; set; }
}