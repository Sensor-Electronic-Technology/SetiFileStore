using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Responses;
namespace SetiFileStore.FileClient;
public record FileData(string Name,byte[] Data);
public class FileService {
    private readonly IHttpClientFactory  _clientFactory;
    private readonly ILogger<FileService> _logger;
    private readonly Uri _baseUrl;

    public FileService(IHttpClientFactory clientFactory,
        IConfiguration configuration,
        ILogger<FileService> logger) {
        var url=configuration["FileServiceUrl"];
        if (string.IsNullOrEmpty(url)) {
            throw new Exception(message: "Missing required configuration: FileServiceUrl. " +
                                         "Please check your appsettings.json file.");
        }
        this._baseUrl = new Uri(url);
        this._clientFactory = clientFactory;
    }
    
    public async Task<string?> UploadFile(FileData data,string domain) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(data.Data);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file",data.Name);
        form.Add(new StringContent(domain), "appDomain");
        //form.Add(new StringContent("purchase_request"), "appDomain");
        HttpResponseMessage response = await client.PostAsync(HttpConstants.FileUploadPath, form);
        if (response.IsSuccessStatusCode) {
            var content =await response.Content.ReadFromJsonAsync<FileUploadResponse>();
            return content?.FileId;
        } else {
            return default;
        }
    }
    
    public async Task<List<string>> UploadMultipleFiles(List<FileData> input,string domain) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        foreach (var fileInput in input) {
            var fileContent = new ByteArrayContent(fileInput.Data);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "files", fileInput.Name);
        }
        form.Add(new StringContent(domain), "appDomain");
        HttpResponseMessage response = await client.PostAsync(HttpConstants.MultiFileUploadPath, form);
        if (response.IsSuccessStatusCode) {
            var content =await response.Content.ReadFromJsonAsync<MultipleFileUploadResponse>();
            return content?.FileIds ?? new List<string>();
        } else {
            return [];
        }
    }
    
    public async Task<FileData?> DownloadFile(string fileId,string domain) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var requestStr=HttpConstants.FileDownloadPath
            .Replace("{appDomain}",domain)
            .Replace("{fileId}",fileId);
        var response = await client.GetAsync(requestStr);
        if (response.IsSuccessStatusCode) {
            var fileName= response.Content.Headers?.ContentDisposition?.FileName;
            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            return new FileData(fileName,fileBytes);
        } else {
            return null;
        }

    }
    
    public async Task<GetFileInfoResponse?> GetFileInfo(string fileId,string domain) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var requestStr=HttpConstants.FileInfoPath
            .Replace("{fileId}",fileId)
            .Replace("{appDomain}",domain);
        var response = await client.GetFromJsonAsync<GetFileInfoResponse>(requestStr);
        return response;
    }

    public async Task<FileData?> DownloadFileStream(string fileId,string domain) {
        var info=await this.GetFileInfo(fileId,domain);
        if (info == null) {
            return null;
        }
        
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var requestStr=HttpConstants.FileDownloadStreamPath
            .Replace("{fileId}",fileId)
            .Replace("{appDomain}",domain);
        await using var stream=await client.GetStreamAsync(requestStr);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return new FileData(info.Filename,memoryStream.ToArray());
    }

    public async Task<bool> DeleteFile(string fileId,string domain) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var requestStr=HttpConstants.FileDeletePath
            .Replace("{appDomain}",domain)
            .Replace("{fileId}",fileId);
        var response = await client.DeleteAsync(requestStr);
        //Console.WriteLine(await response.Content.ReadAsStringAsync());
        return response.IsSuccessStatusCode;
    }
}