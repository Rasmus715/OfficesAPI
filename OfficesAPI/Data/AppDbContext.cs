using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OfficesAPI.Models;

namespace OfficesAPI.Data;

public class AppDbContext
{
    private readonly IMongoCollection<Office> _officesCollection;  
    public AppDbContext(IOptions<OfficesDatabaseSettings> officesDatabaseSettings)
    {
        var mongoClient = new MongoClient(
            officesDatabaseSettings.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(
            officesDatabaseSettings.Value.DatabaseName);

        _officesCollection = mongoDatabase.GetCollection<Office>(
            officesDatabaseSettings.Value.OfficesCollection);
    }

    public virtual IMongoCollection<Office> GetCollection()
        => _officesCollection;
}