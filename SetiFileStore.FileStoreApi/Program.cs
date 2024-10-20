using Domain;
using SetiFileStore.Domain.Model;
using SetiFileStore.Domain.SignatureVerify;
using FastEndpoints;
using FileStorage.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddFastEndpoints()
    .AddResponseCaching()
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.4.15:27017"));

builder.Services.AddSingleton<UploadFileSettings>((serviceProvider) =>
    builder.Configuration.GetSection(nameof(UploadFileSettings)).Get<UploadFileSettings>());

// Add services to the container.
builder.Services.AddSingleton<DatabaseSettings>((serviceProvider) =>
    builder.Configuration.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>());

builder.Services.AddSingleton<FileTypeVerifier>();
builder.Services.AddSingleton<FileValidationService>();
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddHostedService<AutoDeleteService>(); // Auto delete temp file

var app = builder.Build();
app.UseResponseCaching()
    .UseFastEndpoints()
    .UseSwagger()
    .UseSwaggerUI();
app.Run();