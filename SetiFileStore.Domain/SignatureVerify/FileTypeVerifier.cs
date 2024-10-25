using SetiFileStore.Domain.SignatureVerify.FileTypes;


namespace SetiFileStore.Domain.SignatureVerify;

public class FileTypeVerifier {
    public FileTypeVerifier() { }

    private static FileTypeVerifyResult Unknown = new FileTypeVerifyResult() {
        Name = "Unknown", Description = "Unknown File Type", IsVerified = false
    };

    private byte[] GetFirstBytes(byte[] file) {
        const int chunkSize = 64;
        var instanceBuffer = file.Take(chunkSize).ToArray();
        return instanceBuffer;
    }

    public FileTypeVerifyResult Verify(byte[] file, string extension) {
        FileType fileType = FileTypeClass(extension);
        var bytes = GetFirstBytes(file);
        var result = fileType.Verify(bytes);
        return result?.IsVerified == true ? result : Unknown;
    }

    private FileType FileTypeClass(string extension) {
        extension = extension.ToLowerInvariant();
        FileType fileType;
        switch (extension) {
            case ".jpg":
                fileType = new Jpg();
                return fileType;
            case ".png":
                fileType = new Png();
                return fileType;
            case ".jpeg":
                fileType = new Jpeg();
                return fileType;
            case ".bmp":
                fileType = new Bmp();
                return fileType;
            case ".doc":
                fileType = new Doc();
                return fileType;
            case ".docx":
                fileType = new Docx();
                return fileType;
            case ".gif":
                fileType = new Gif();
                return fileType;
            case ".pdf":
                fileType = new Pdf();
                return fileType;
            case ".xls":
                fileType = new Xls();
                return fileType;
            case ".xlsx":
                fileType = new Xlsx();
                return fileType;
            case ".xml":
                fileType = new Xml();
                return fileType;
            case ".svg":
                fileType = new Svg();
                return fileType;
            default:
                break;
        }

        Exception exception = new Exception("");
        throw exception;
    }
}