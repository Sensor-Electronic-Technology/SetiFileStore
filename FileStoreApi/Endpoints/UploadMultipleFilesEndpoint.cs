using Domain.Contracts.Requests;
using Domain.Contracts.Responses;
using FastEndpoints;
using FileStorage.Services;
using MongoDB.Bson;

namespace FileStoreApi.Endpoints;

public class UploadMultipleFilesEndpoint:Endpoint<MultipleFileUploadRequest,MultipleFileUploadResponse> {
    private readonly FileStorageService _fileStore;
    
    public UploadMultipleFilesEndpoint(FileStorageService fileStore) {
        this._fileStore=fileStore;
    }
    
    public override void Configure() {
        Post("api/file/upload/multiple");
        AllowAnonymous();
        AllowFileUploads();
    }
    
    public override async Task HandleAsync(MultipleFileUploadRequest request, CancellationToken cancellationToken) {
        if(!request.Files.Any() || string.IsNullOrEmpty(request.AppDomain)) {
            ThrowError("No files uploaded");
        }
        ThrowIfAnyErrors();
        List<string> fileIds = [];
        bool failed = false;
        foreach(var file in request.Files) {
            var filename = file.FileName;
            var contentType= file.ContentType;
            var fileStream = file.OpenReadStream();
            var result = await _fileStore.UploadSmallFileFromStreamAsync(filename, contentType,request.AppDomain,fileStream, cancellationToken);
            if (result.IsSuccessful) {
                fileIds.Add(result.ObjectId);
            } else {
                failed = true;
                break;
            }
        }

        if (failed) {
            if(fileIds.Any()) {
                foreach(var fileId in fileIds) {
                    if(ObjectId.TryParse(fileId, out var id)) {
                        await _fileStore.DeleteFileAsync(id,request.AppDomain, cancellationToken);
                    }
                }
            }
        } else {
            await SendAsync(new MultipleFileUploadResponse() { FileIds = fileIds }, cancellation: cancellationToken);
        }
    }


}