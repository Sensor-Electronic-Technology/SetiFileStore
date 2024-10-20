using FastEndpoints;
using FileStorage.Services;
using MongoDB.Bson;
using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;

namespace SetiFileStore.FileStoreApi.Endpoints;

public class DownloadFileStreamEndpoint:Endpoint<FileDownloadRequest> {
    private readonly FileStorageService _fileStore;

    public DownloadFileStreamEndpoint(FileStorageService fileStore) {
        _fileStore = fileStore;
    }
    
    public override void Configure() {
        Get(HttpConstants.FileDownloadStreamPath);
        AllowAnonymous();
    }

    public override async Task HandleAsync(FileDownloadRequest request, CancellationToken ct) {
        var parsedObjectId = new ObjectId(request.FileId.Trim());

        var fileInfo = await this._fileStore.GetFileInfoById(parsedObjectId,request.AppDomain);
        if (fileInfo == null) {
            ThrowError("File not found", 404);
        }
        ThrowIfAnyErrors();
        var tempFilePath = Directory.GetCurrentDirectory() + "\\Temp\\" + fileInfo.Filename;
        Stream destination = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite);
        await this._fileStore.DownloadToStreamAsync(parsedObjectId,request.AppDomain,destination, ct);
        string? contentType = null;
        string? fileName = null;
        var metadata = fileInfo.Metadata;
        if (metadata != null) {
            contentType = metadata.GetElement("ContentType").Value.ToString();
            fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
        }
        destination.Seek(0, SeekOrigin.Begin);
        
        await SendStreamAsync(destination,
            fileName,
            fileInfo.Length,
            "application/octet-stream",enableRangeProcessing:true,
            cancellation: ct);
    }
}