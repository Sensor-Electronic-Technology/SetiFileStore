using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using FastEndpoints;
using FileStorage.Services;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class CreateAppDomainEndpoint:Endpoint<CreateAppDomainRequest,EmptyResponse> {
    
    private readonly FileStorageService _fileStore;
    
    public CreateAppDomainEndpoint(FileStorageService fileStore) {
        this._fileStore=fileStore;
    }
    
    public override void Configure() {
        Post(HttpConstants.AppDomainPath);
        //Post("api/appdomain");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CreateAppDomainRequest request, CancellationToken cancellationToken) {
        if(string.IsNullOrEmpty(request.AppDomain) || string.IsNullOrEmpty(request.Description)) {
            ThrowError("AppDomain and Description are required");
        }
        ThrowIfAnyErrors();
        await this._fileStore.CreateSetiAppDomain(request.AppDomain,request.Description);
        await SendOkAsync(cancellationToken);
    }

}