using SetiFileStore.Domain.Model;
using SetiFileStore.Domain.SignatureVerify;

namespace Domain;

public class FileValidationService {
    private readonly FileTypeVerifier _verifier;
    private readonly UploadFileSettings _settings;
    
    public FileValidationService(FileTypeVerifier verifier, UploadFileSettings settings) {
        this._verifier = verifier;
        this._settings = settings;
    }

    public ValidationFileEnum ValidateFileWhiteList(string filename) {
        var extension = Path.GetExtension(filename).ToLowerInvariant();
        var whiteListExtensions = _settings.WhiteListExtensions.Split(",");
        if (!whiteListExtensions.Contains(extension)) {
            return ValidationFileEnum.FileNotSupported;
        }

        return ValidationFileEnum.Ok;
    }
    
    public ValidationFileEnum ValidateFileSignature(byte[] file,string extension) {
        var signatureValidationExtensions = _settings.SignatureValidationExtensions.Split(",");
        if (signatureValidationExtensions.Contains(extension)) {
            var verifyResult = this._verifier.Verify(file, extension);
            if (!verifyResult.IsVerified) {
                return ValidationFileEnum.InvalidSignature;
            }
        }
        return ValidationFileEnum.Ok;
    }
    
    public ValidationFileEnum ValidateFileMaxLength(long lengthOfFile,
        FileSizeEnum fileSize = FileSizeEnum.Small) {
        return fileSize switch
        {
            FileSizeEnum.Small when lengthOfFile > this._settings.MaxSizeLimitSmallFile => ValidationFileEnum
                .FileIsTooLarge,
            FileSizeEnum.Large when lengthOfFile > this._settings.MaxSizeLimitLargeFile => ValidationFileEnum
                .FileIsTooLarge,
            _ => ValidationFileEnum.Ok
        };
    }
    
    public ValidationFileEnum ValidateFileMinLength(long lengthOfFile,
        FileSizeEnum fileSize = FileSizeEnum.Small)
    {
        return fileSize switch
        {

            FileSizeEnum.Small when lengthOfFile < this._settings.MinSizeLimitSmallFile => ValidationFileEnum
                .FileIsTooSmall,

            FileSizeEnum.Large when lengthOfFile < this._settings.MinSizeLimitLargeFile => ValidationFileEnum
                .FileIsTooSmall,
            _ => ValidationFileEnum.Ok
        };
    }
    
    public ValidationFileEnum ValidateFileLength(long lengthOfFile,
        FileSizeEnum fileSize = FileSizeEnum.Small)
    {
        var result = ValidateFileMaxLength(lengthOfFile, fileSize);
        return result == ValidationFileEnum.Ok ? result : ValidateFileMinLength(lengthOfFile, fileSize);
    }
    
    public ValidationFileEnum ValidateFile(byte[] file, string? fileName,
        long? lengthOfFile = null,
        FileSizeEnum fileSize = FileSizeEnum.Small, CancellationToken cancellationToken = default)
    {
        var length = lengthOfFile ?? file.Length;
        if (string.IsNullOrEmpty(fileName))
        {
            // If the code runs to this location, it means that no files have been saved
            return ValidationFileEnum.FileNotFound;
        }

        var validateFileExtenstionResult = ValidateFileWhiteList(fileName);
        if (validateFileExtenstionResult != ValidationFileEnum.Ok)
        {
            return validateFileExtenstionResult;
        }


        var validateFileLengthResult = ValidateFileLength(length, fileSize);
        if (validateFileLengthResult != ValidationFileEnum.Ok)
        {
            return validateFileLengthResult;
        }

        var fileExtension = Path.GetExtension(fileName);
        var validateFileSignatureResult = ValidateFileSignature(file, fileExtension);
        if (validateFileSignatureResult != ValidationFileEnum.Ok)
        {
            return validateFileLengthResult;
        }

        return ValidationFileEnum.Ok;
    }
    
    public string GetValidationMessage(ValidationFileEnum validationFileEnum)
    {
        switch (validationFileEnum)
        {
            case ValidationFileEnum.FileNotFound:
                // If the code runs to this location, it means that no files have been saved
                return "No files data in the request.";

            case ValidationFileEnum.FileIsTooLarge:
                // If the code runs to this location, it means that no files have been saved
                return "The file is too large.";

            case ValidationFileEnum.FileIsTooSmall:
                // If the code runs to this location, it means that no files have been saved
                return "The file is too small.";

            case ValidationFileEnum.FileNotSupported:
                // If the code runs to this location, it means that no files have been saved
                return "The file is not supported.";

            case ValidationFileEnum.InvalidSignature:
                // If the code runs to this location, it means that no files have been saved
                return "The file extension is not trusted.";

            case ValidationFileEnum.Ok:
            default:
                return "";
        }
    }
    
}