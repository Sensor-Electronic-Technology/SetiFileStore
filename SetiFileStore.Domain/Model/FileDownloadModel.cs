﻿using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace SetiFileStore.Domain.Model;

public class FileDownloadModel {
    public ObjectId ObjectId { get; set; }
    public byte[] FileBytes { get; set; } = null!;
    public GridFSFileInfo FileInfo { get; set; } = null!;
}