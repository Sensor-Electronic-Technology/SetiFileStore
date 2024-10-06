using Domain.Contracts.Requests;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FileStoreApi.Endpoints;

public class DownloadFileEndpoint:Endpoint<FileDownloadRequest> {
    private readonly FileStorageService _fileStore;

    public DownloadFileEndpoint(FileStorageService fileStore) {
        _fileStore = fileStore;
    }

    public override void Configure() {
        Get("/api/files/download/{fileId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FileDownloadRequest request, CancellationToken cancellationToken) {
        /*Console.WriteLine($"FileId: {request.FileId}");
        Console.WriteLine($"AppDomain: {request.AppDomain}");*/
        var parsedObjectId = new ObjectId(request.FileId.Trim());

        var fileInfo = await this._fileStore.GetFileInfoById(parsedObjectId,request.AppDomain);
        if (fileInfo == null) {
            ThrowError("File not found", 404);
        }
        ThrowIfAnyErrors();
        var fileBytes = await this._fileStore.DownloadAsBytesAsync(parsedObjectId,request.AppDomain, cancellationToken);
        string? contentType = null;
        string? fileName = null;
        var metadata = fileInfo.Metadata;
        if (metadata != null) {
            contentType = metadata.GetElement("ContentType").Value.ToString();
            fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
        }
        await SendBytesAsync(fileBytes,fileName, contentType ?? "application/octet-stream", cancellation: cancellationToken);

    }
    
}