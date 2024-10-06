using Domain.Contracts.Requests;
using Domain.Contracts.Responses;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileStoreApi.Endpoints;

public class FileUploadEndpoint:Endpoint<FileUploadRequest, FileUploadResponse>
{
    private readonly FileStorageService _fileStore;

    public FileUploadEndpoint(FileStorageService fileStore) {
        _fileStore = fileStore;
    }

    public override void Configure() {
        Post("api/file/upload");
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(FileUploadRequest request, CancellationToken cancellationToken) {
        var filename = request.File.FileName;
        var contentType= request.File.ContentType;
        var fileStream = request.File.OpenReadStream();
        var result = await _fileStore.UploadSmallFileFromStreamAsync(filename, contentType,request.AppDomain,fileStream, cancellationToken);
        await SendAsync(new FileUploadResponse() { FileId = result.ObjectId }, cancellation: cancellationToken);

        //await this._fileStore.UploadFromStreamAsync()
        //await _fileStore.UploadFromStreamAsync(fileStream);
    }
    
}