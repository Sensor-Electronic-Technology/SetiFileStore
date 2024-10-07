using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class FileDeleteEndpoint:Endpoint<FileDeleteRequest> {
    private readonly FileStorageService _fileStore;
    
    public FileDeleteEndpoint(FileStorageService fileStore) {
        this._fileStore=fileStore;
    }
    
    public override void Configure() {
        Delete(HttpConstants.FileDeletePath);
        AllowAnonymous();
    }
    
    public override async Task HandleAsync(FileDeleteRequest request, CancellationToken cancellationToken) {
        if(string.IsNullOrEmpty(request.FileId) || string.IsNullOrEmpty(request.AppDomain)) {
            ThrowError("Invalid request, FileId and AppDomain are required");
        }
        ThrowIfAnyErrors();
        var parsedObjectId = new ObjectId(request.FileId.Trim());
        var result = await _fileStore.DeleteFileAsync(parsedObjectId,request.AppDomain, cancellationToken);
        if(result) {
            await SendAsync(new OkResult(), cancellation: cancellationToken);
        } else {
            ThrowError("Failed to delete file", 500);
        }
    }
}