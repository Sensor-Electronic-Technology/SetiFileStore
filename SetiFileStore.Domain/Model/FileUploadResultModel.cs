﻿namespace SetiFileStore.Domain.Model;

public class FileUploadResultModel {
    public string ObjectId { get; set; }
    public string? FileName { get; set; } = null!;
    public bool IsSuccessful { get; set; } = true;
    public string ErrorMessage { get; set; } = null!;
}