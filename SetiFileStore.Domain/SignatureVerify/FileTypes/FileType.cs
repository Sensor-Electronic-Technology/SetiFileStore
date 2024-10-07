namespace SetiFileStore.Domain.SignatureVerify.FileTypes;

public class FileTypeVerifyResult {
    public string Name { get; set; }=null!;
    public string Description { get; set; }=null!;
    public bool IsVerified { get; set; }
}

public abstract class FileType {
    protected string Description { get; set; } = null!;
    protected string Name { get; set; } = null!;
    private List<string> Extensions { get; } = [];
    private List<byte[]> Signatures { get; } = [];
    
    public int SignatureLength => Signatures.Max(m=>m.Length);
    
    protected FileType AddSignatures(params byte[][] signatures) {
        Signatures.AddRange(signatures);
        return this;
    }
    
    protected FileType AddExtensions(params string[] extensions) {
        Extensions.AddRange(extensions);
        return this;
    }
    
    public FileTypeVerifyResult Verify(byte[] headerBytes) {
        return new FileTypeVerifyResult() {
            Name=Name,
            Description=Description,
            IsVerified = Signatures.Any(sig=>
                headerBytes.Take(sig.Length)
                    .SequenceEqual(sig))
        };
    }
    
    /*public bool IsMatch(byte[] data) {
        if (data.Length < SignatureLength) return false;
        foreach (var signature in Signatures) {
            if (data.Take(signature.Length).SequenceEqual(signature)) return true;
        }
        return false;
    }*/

}