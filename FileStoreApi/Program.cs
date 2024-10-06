using Domain;
using Domain.Model;
using Domain.SignatureVerify;
using FastEndpoints;
using FileStorage.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web.Resource;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints()
    .AddSwaggerGen()
    .AddEndpointsApiExplorer();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetConnectionString("DefaultConnection") 
                                                            ?? "mongodb://172.20.3.41:27017"));

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

app.UseFastEndpoints()
    .UseSwagger()
    .UseSwaggerUI();
app.Run();