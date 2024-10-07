// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SetiFileStore.Domain.Contracts.Requests;
using SetiFileStore.Domain.Model;
using SetiFileStore.FileClient;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

await CreateAppDomain();
//await DownloadFileTest();
//await UploadFileTest();
//await CheckFileInfoTest();
//await TestDownloadFile_FileService();
//await TestUploadFile_FileService();
//await TestDeleteFile_FileService();
//await TestUploadMultipleFile_FileService();

/*async Task CheckFileInfoTest() {
    var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    FileService fileService = new FileService(client);
    var result=await fileService.GetFileInfo("6702d929f11c2b78c75cb83d");
    if(result!=null) {
        Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions() {
            WriteIndented = true
        }));
    } else {
        Console.WriteLine("File info failed");
    }
}

async Task TestDownloadFile_FileService() {
    var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    FileService fileService = new FileService(client);
    var result=await fileService.DownloadFile("6702c8754432056fe8cb18fa");
    if(result!=null) {
        Console.WriteLine("File downloaded");
        var path = Path.Combine("C:\\Users\\aelmendo\\Downloads\\",result.Name);
        await using var fs = File.Create(path, result.Data.Length);
        await fs.WriteAsync(result.Data);
    } else {
        Console.WriteLine("File download failed");
    }
}

async Task TestDeleteFile_FileService() {
    var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    FileService fileService = new FileService(client);
    if (await fileService.DeleteFile("6702d929f11c2b78c75cb83d")) {
        Console.WriteLine("Delete Successful");
    } else {
        Console.WriteLine("Delete Failed");
    }
}

async Task TestUploadFile_FileService() {
    var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    FileService fileService = new FileService(client);
    var fileData = new FileData("PurchaseRequest-2.pdf", await File.ReadAllBytesAsync(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest-2.pdf"));
    var result = await fileService.UploadFile(fileData);
    if (result != null) {
        Console.WriteLine($"File uploaded. FileId: {result}");
    } else {
        Console.WriteLine("File upload failed");
    }
}

async Task TestUploadMultipleFile_FileService() {
    var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/");
    FileService fileService = new FileService(client);
    List<FileData> data = [
        new FileData("PurchaseRequest-2.pdf",
            await File.ReadAllBytesAsync(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest-2.pdf")),
        new FileData("PurchaseRequest.pdf",
            await File.ReadAllBytesAsync(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest.pdf"))
    ];
    var result = await fileService.UploadMultipleFiles(data);
    if (result.Any()) {
        foreach (var output in result) {
            Console.WriteLine($"File uploaded. FileId: {output}");
        }
    } else {
        Console.WriteLine("File upload failed");
    }
}*/

async Task CreateAppDomain() {
    var client = new MongoClient("mongodb://172.20.4.15:27017");
    var database = client.GetDatabase("seti-file-store");
    var collection = database.GetCollection<SetiAppDomain>("app_domains");
    var domain=new SetiAppDomain() {
        _id=ObjectId.GenerateNewId(),
        Name="purchase_request",
        Description="Purchase Request domain. " +
                    "For the file storage of quotes and other documents related to purchase requests."
    };
    await collection.InsertOneAsync(domain);
    Console.WriteLine("SetiFileStore.Domain inserted");
}
async Task DownloadFileTest() {
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/api/");
    var response = await client.GetAsync($"files/download/6702c8754432056fe8cb18fa?appDomain=purchase_request");
    response.EnsureSuccessStatusCode();
    var fileName= response.Content.Headers?.ContentDisposition?.FileName;
    if (string.IsNullOrEmpty(fileName)) {
        return;
    }
    var fileInfo = new System.IO.FileInfo(fileName);
    await using var ms = await response.Content.ReadAsStreamAsync();
    //append path here

    var path = Path.Combine("C:\\Users\\aelmendo\\Downloads\\",fileInfo.Name);
    await using var fs = File.Create(path);
    ms.Seek(0, SeekOrigin.Begin);
    await ms.CopyToAsync(fs);
}

async Task UploadFileTest() {
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://localhost:5065/api/");
    var uploadRequest = new FileUploadRequest() {
        AppDomain = "purchase_request",
    };
    await using var fileStream= File.OpenRead(@"C:\Users\aelme\Documents\PurchaseRequestData\PurchaseRequest-2.pdf");
    using var form = new MultipartFormDataContent();
    using var streamContent = new StreamContent(fileStream);
    var filebytes=await streamContent.ReadAsByteArrayAsync();
    using var fileContent = new ByteArrayContent(filebytes);
    fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
    form.Add(fileContent, "file","PurchaseRequest-2.pdf");
    form.Add(new StringContent(uploadRequest.AppDomain), "appDomain");
    HttpResponseMessage response = await client.PostAsync("file/upload", form);
    response.EnsureSuccessStatusCode();
}