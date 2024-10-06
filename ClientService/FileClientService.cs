using System.Net.Http.Headers;
using System.Text.Json;
using FileStorage.Model;
using Microsoft.Extensions.Configuration;

/*using Radzen;
using FileInfo = Radzen.FileInfo;*/
namespace ClientService;

public class FileClientService {
    private readonly IHttpClientFactory  _clientFactory;
    private readonly Uri _baseUrl;

    
    public FileClientService(IHttpClientFactory client,IConfiguration configuration) {
        this._clientFactory = client;
        this._baseUrl = new Uri(configuration["FileServiceUrl"] ?? "http://localhost:8080/FileStorage/");
    }
    
    public async Task<string> UploadFile(string path) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        
        using var form = new MultipartFormDataContent();
        await using var fs = File.OpenRead(path);
        using var streamContent = new StreamContent(fs);
        using var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        // "file" parameter name should be the same as the server side input parameter name
        form.Add(fileContent, "file", Path.GetFileName(path));
        HttpResponseMessage response = await client.PostAsync("UploadFile", form);
    }
    
    public async Task<string?> UploadFile(FileInfo file) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        using var form = new MultipartFormDataContent();
        //file.OpenRead();
        /*await using var stream = file.OpenReadStream(1048576000);*/
        await using var stream = file.OpenRead();
        using var streamContent = new StreamContent(stream);
        using var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
        form.Add(fileContent, "file", file.Name);
        HttpResponseMessage response = await client.PostAsync("UploadFile", form);
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode) {
            var content =await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FileUploadResultModel>(content);
            if(result!=null) {
                return result.ObjectId;
            } else {
                return default;
            }
        } else {
            return default;
        }
    }

    public async Task<List<string>> UploadMultipleFiles(IList<FileInfo> files) {
        using var form = new MultipartFormDataContent();
        if(files.Count==0) return [];
        if (files.Count == 1) {
            var result=await UploadFile(files[0]);
            return string.IsNullOrEmpty(result) ? []:[result];
        }
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        
        foreach (var file in files) {
            /*var stream = file.OpenReadStream(1048576000);*/
            var stream=file.OpenRead();
            var streamContent = new StreamContent(stream);
            var fileContent = new ByteArrayContent(await streamContent.ReadAsByteArrayAsync());
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "files", file.Name);
        }
        var response = await client.PostAsync("UploadMultipleFiles", form);
        response.EnsureSuccessStatusCode();
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode) {
            var content =await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<FileUploadResultModel>>(content);
            if(result!=null) {
                return result.Select(e=>e.ObjectId).ToList();
            } else {
                return [];
            }
        } else {
            return [];
        }
    }

    public async Task DownloadFile(string objectId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var response = await client.GetAsync($"DownloadFile?objectId={objectId}");
        response.EnsureSuccessStatusCode();
       var fileName= response.Content.Headers?.ContentDisposition?.FileName;
       if (string.IsNullOrEmpty(fileName)) {
           return;
       }
       var fileInfo = new System.IO.FileInfo(fileName);
       await using var ms = await response.Content.ReadAsStreamAsync();
       //append path here
       await using var fs = File.Create(fileInfo.FullName);
       ms.Seek(0, SeekOrigin.Begin);
       await ms.CopyToAsync(fs);
    }

    public async Task<string?> GetFileName(string objectId) {
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var response=await client.GetAsync($"GetFileInfo?objectId={objectId}");
        response.EnsureSuccessStatusCode();
        var content =await response.Content.ReadAsStringAsync();
        var document = JsonSerializer.Deserialize<JsonDocument>(content);
        if (document != null) {
            if(document.RootElement.TryGetProperty("filename", out var filename)) {
                return filename.GetString();
            }
        }
        return default;
    }

    public async Task DownloadFileStream(string objectId) {
        var filename=await GetFileName(objectId);
        if (string.IsNullOrEmpty(filename)) {
            return;
        }
        using var client = this._clientFactory.CreateClient();
        client.BaseAddress = this._baseUrl;
        var fileStream = await client.GetStreamAsync($"DownloadFileStream?objectId={objectId}");
        var path=Path.Combine(@"C:\Users\aelme\Documents\PurchaseRequestData\Downloads", filename);
        await using FileStream outputFileStream = new FileStream(path, FileMode.CreateNew);
        await fileStream.CopyToAsync(outputFileStream);
    }
}