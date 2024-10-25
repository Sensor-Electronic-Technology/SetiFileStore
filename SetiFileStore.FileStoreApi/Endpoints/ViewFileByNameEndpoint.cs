/*using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Requests;
using FastEndpoints;
using FileStorage.Services;
namespace SetiFileStore.FileStoreApi.Endpoints;

public class ViewFileByNameEndpoint:Endpoint<FileDownloadRequest> {
    
    private readonly FileStorageService _fileStore;
    
    public ViewFileByNameEndpoint(FileStorageService fileStore) {
        this._fileStore=fileStore;
    }
    
    public override void Configure() {
        Get("api/file/view/{appDomain}/{fileId}");
        //Post("api/appdomain");
        AllowAnonymous();
    }

    public override async Task HandleAsync(FileDownloadRequest request, CancellationToken cancellationToken) {
        /*var fileBytes = await this._fileStore.DownloadAsBytesAsync(parsedObjectId,request.AppDomain, cancellationToken);
        string? contentType = null;
        string? fileName = null;
        var metadata = fileInfo.Metadata;
        if (metadata != null) {
            contentType = metadata.GetElement("ContentType").Value.ToString();
            fileName = metadata.GetElement("UntrustedFileName").Value.ToString();
        }#1#
        
        System.Net.Mime.ContentDisposition cd = new System.Net.Mime.ContentDisposition {
            FileName = request.FileId,
            Inline = true  // false = prompt the user for downloading;  true = browser to try to show the file inline
        };
        HttpContext.Response.Headers.Append("Content-Disposition", cd.ToString());
        HttpContext.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        await SendFileAsync(fileBytes,fileName,"application/pdf", cancellation: cancellationToken);
    }
    
}*/