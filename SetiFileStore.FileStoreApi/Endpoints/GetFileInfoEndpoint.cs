using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using SetiFileStore.Domain.Contracts.Responses;
using FastEndpoints;
using FileStorage.Services;
using MongoDB.Bson;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class GetFileInfoEndpoint:Endpoint<GetFileInfoRequest, GetFileInfoResponse> {
    private readonly FileStorageService _fileStore;
    
    public GetFileInfoEndpoint(FileStorageService fileStore) {
        this._fileStore=fileStore;
    }
    
    public override void Configure() {
        Get(HttpConstants.FileInfoPath);
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(GetFileInfoRequest request, CancellationToken cancellationToken) {
        if(string.IsNullOrEmpty(request.FileId) || string.IsNullOrEmpty(request.AppDomain)) {
            ThrowError("FileId and AppDomain are required", 400);
        }
        ThrowIfAnyErrors();
        var parsedObjectId = new ObjectId(request.FileId.Trim());
        var fileInfo = await this._fileStore.GetFileInfoById(parsedObjectId,request.AppDomain);
        if (fileInfo == null) {
            ThrowError("File not found", 404);
        }
        var metadata = fileInfo.Metadata;
        
        if (metadata != null) {
            var response= new GetFileInfoResponse() {
                ContentType = metadata.GetElement("ContentType").Value.ToString() ?? "application/octet-stream",
                Filename = metadata.GetElement("UntrustedFileName").Value.ToString() ?? "file",
            };
            await SendAsync(response, cancellation:cancellationToken);
        } else {
            await SendNotFoundAsync(cancellationToken);
        }
    }
}