﻿namespace SetiFileStore.Domain.Contracts;

public class HttpConstants {
    public static string FileUploadPath = "file/upload";
    public static string LargeFileUploadPath = "file/upload/large";
    public static string AppDomainPath = "api/appdomain/create";
    public static string MultiFileUploadPath = "api/file/upload/multiple";
    public static string MultiLargeFileUploadPath = "api/file/upload/multiple/large";
    public static string FileDeletePath = "api/file/delete/{appDomain}/{fileId}";
    public static string FileDownloadPath = "/api/file/download/{appDomain}/{fileId}";
    public static string FileInfoPath = "api/file/info/{appDomain}/{fileId}";
}