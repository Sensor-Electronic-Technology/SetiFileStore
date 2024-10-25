using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using SetiFileStore.Domain.Contracts.Responses;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Mvc;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class FileUploadEndpoint:Endpoint<FileUploadRequest, FileUploadResponse>
{
    private readonly FileStorageService _fileStore;

    public FileUploadEndpoint(FileStorageService fileStore) {
        _fileStore = fileStore;
    }

    public override void Configure() {
        Post(HttpConstants.FileUploadPath);
        AllowAnonymous();
        AllowFileUploads();
    }

    public override async Task HandleAsync(FileUploadRequest request, CancellationToken cancellationToken) {
        var filename = request.File.FileName;
        var contentType= request.File.ContentType;
        var fileStream = request.File.OpenReadStream();
        var result = await _fileStore.UploadSmallFileFromStreamAsync(filename, contentType,request.AppDomain,fileStream, cancellationToken);
        /*Console.WriteLine($"File uploaded: {result.ObjectId}");*/
        await SendAsync(new FileUploadResponse() { FileId = result.ObjectId }, cancellation: cancellationToken);
        //await this._fileStore.UploadFromStreamAsync()
        //await _fileStore.UploadFromStreamAsync(fileStream);
    }
    
}