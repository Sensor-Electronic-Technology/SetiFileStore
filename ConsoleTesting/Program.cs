// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using SetiFileStore.Domain.Contracts.Requests;
using SetiFileStore.Domain.Model;
using MongoDB.Bson;
using MongoDB.Driver;
using SetiFileStore.Domain.Contracts;
using SetiFileStore.FileClient;

//await CreateAppDomain();
//await DownloadFileTest();
//await UploadFileTest();
//await CheckFileInfoTest();
//await TestDownloadFile_FileService();
//await TestUploadFile_FileService();
//await TestDeleteFile_FileService();
//await TestUploadMultipleFile_FileService();
var fileData=await DownloadFileTest();
if(fileData!=null) {
    await using var fs = File.Create("C:\\Users\\aelmendo\\Downloads\\"+fileData.Name);
    await fs.WriteAsync(fileData.Data);
    Console.WriteLine($"File Downloaded, Check downloads  File: {fileData.Name}");
} else {
    Console.WriteLine("File data null");
}


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
async Task<FileData?> DownloadFileTest() {
    using var client = new HttpClient();
    client.BaseAddress = new Uri("http://172.20.4.15:8080/");
    var httpPath=HttpConstants.FileDownloadPath
        .Replace("{appDomain}","purchase_request")
        .Replace("{fileId}","6707d397b6699e1989a04023");
    var response = await client.GetAsync(httpPath);
    response.EnsureSuccessStatusCode();
    if (response.IsSuccessStatusCode) {
        var fileName= response.Content.Headers?.ContentDisposition?.FileName;
        //var responseText=await response.Content.ReadAsStringAsync();
        //Console.WriteLine(responseText);
        if (string.IsNullOrEmpty(fileName)) {
            Console.WriteLine("Result Empty");
            return null;
        }
        var fileInfo = new System.IO.FileInfo(fileName);
        var bytes = await response.Content.ReadAsByteArrayAsync();
        return new FileData(fileName, bytes);
        /*await using var fs = File.Create("C:\\Users\\aelmendo\\Downloads\\"+fileInfo.Name);
        await fs.WriteAsync(bytes);
        Console.WriteLine($"File Downloaded, Check downloads  File: {fileInfo.Name}");*/
        /*await using var ms = await response.Content.ReadAsStreamAsync();
        var path = Path.Combine("C:\\Users\\aelmendo\\Downloads\\",fileInfo.Name);
        await using var fs = File.Create(path);
        ms.Seek(0, SeekOrigin.Begin);
        await ms.CopyToAsync(fs);
        Console.WriteLine("File Downloaded, Check downloads");*/
    } else {
        Console.WriteLine("Download Failed");
        return null;
    }
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