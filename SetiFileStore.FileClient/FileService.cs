using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SetiFileStore.Domain.Contracts;
using SetiFileStore.Domain.Contracts.Responses;

namespace SetiFileStore.FileClient;

public record FileData(string Name,byte[] Data);

public class FileService {
    private readonly IConfiguration _configuration;
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
        this._configuration = configuration;
        this._clientFactory = clientFactory;
    }
    
    /*public FileService(HttpClient client) {
        this._client = client;
        //this._configuration = configuration;
    }*/
    
    public async Task<string?> UploadFile(FileData data) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(data.Data);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file",data.Name);
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
        }
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
    
    public async Task<List<string>> UploadMultipleFiles(List<FileData> input) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        foreach (var fileInput in input) {
            var fileContent = new ByteArrayContent(fileInput.Data);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "files", fileInput.Name);
        }
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
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
    
    public async Task<FileData?> DownloadFile(string fileId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
        }
        //var requestStr=HttpConstants.FileDownloadPath.Replace("{appDomain}","purchase_request").Replace("{fileId}",fileId);
        var requestStr=HttpConstants.FileDownloadPath
            .Replace("{appDomain}",domain)
            .Replace("{fileId}",fileId);
        var response = await client.GetAsync(requestStr);
        if (response.IsSuccessStatusCode) {
            var fileName= response.Content.Headers?.ContentDisposition?.FileName;
            if (string.IsNullOrEmpty(fileName)) {
                return null;
            }
            /*await using var stream = await response.Content.ReadAsB();*/
            var fileBytes = await response.Content.ReadAsByteArrayAsync();
            return new FileData(fileName,fileBytes);
        } else {
            return null;
        }

    }
    
    public async Task<GetFileInfoResponse?> GetFileInfo(string fileId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
        }
        //var requestStr=HttpConstants.FileInfoPath.Replace("{fileId}",fileId).Replace("{appDomain}","purchase_request");
        var requestStr=HttpConstants.FileInfoPath
            .Replace("{fileId}",fileId)
            .Replace("{appDomain}",domain);
        var response = await client.GetFromJsonAsync<GetFileInfoResponse>(requestStr);
        return response;
    }

    public async Task<FileData?> DownloadFileStream(string fileId) {
        var info=await this.GetFileInfo(fileId);
        if (info == null) {
            return null;
        }
        
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
        }
        
        var requestStr=HttpConstants.FileDownloadStreamPath
            .Replace("{fileId}",fileId)
            .Replace("{appDomain}",domain);
        await using var stream=await client.GetStreamAsync(requestStr);
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        return new FileData(info.Filename,memoryStream.ToArray());
    }

    public async Task<bool> DeleteFile(string fileId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var domain=this._configuration["AppDomain"];
        if (string.IsNullOrEmpty(domain)) {
            throw new Exception(message: "Missing required configuration: AppDomain. " +
                                         "Please check your appsettings.json file.");
        }
        var requestStr=HttpConstants.FileDeletePath
            .Replace("{appDomain}",domain)
            .Replace("{fileId}",fileId);
        var response = await client.DeleteAsync(requestStr);
        //Console.WriteLine(await response.Content.ReadAsStringAsync());
        return response.IsSuccessStatusCode;
    }
}