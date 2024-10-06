// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Contracts.Requests;
using Domain.Model;
using Microsoft.AspNetCore.Http;
using MongoDB.Bson;
using MongoDB.Driver;

//await DownloadFileTest();
await UploadFileTest();

async Task CreateAppDomain() {
var client = new MongoClient("mongodb://172.20.3.41:27017");
    var database = client.GetDatabase("seti-file-store");
    var collection = database.GetCollection<SetiAppDomain>("app_domains");
    var domain=new SetiAppDomain() {
        _id=ObjectId.GenerateNewId(),
        Name="purchase_request",
        Description="Purchase Request domain. " +
                    "For the file storage of quotes and other documents related to purchase requests."
    };
    await collection.InsertOneAsync(domain);
    Console.WriteLine("Domain inserted");
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