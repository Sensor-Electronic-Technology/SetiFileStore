using MongoDB.Bson;

namespace SetiFileStore.Domain.Model;

public class SetiAppDomain {
    public ObjectId _id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}