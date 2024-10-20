using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class DownloadFileInlineEndpoint:Endpoint<FileDownloadRequest> {
    private readonly FileStorageService _fileStore;

    public DownloadFileInlineEndpoint(FileStorageService fileStore) {
        _fileStore = fileStore;
    }

    public override void Configure() {
        Get(HttpConstants.FileDownloadInlinePath);
        AllowAnonymous();
    }

    public override async Task HandleAsync(FileDownloadRequest request, CancellationToken cancellationToken) {
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
        System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition {
            FileName = fileName,
            Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
        };
        HttpContext.Response.Headers.Append("Content-Disposition", cd.ToString());
        HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        await SendBytesAsync(fileBytes,"application/pdf", cancellation: cancellationToken);

    }
    
}