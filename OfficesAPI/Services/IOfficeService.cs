using AutoMapper;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficesAPI.Data;
using OfficesAPI.Models;
using OfficesAPI.ViewModels;

namespace OfficesAPI.Services;

public interface IOfficeService
{
    Task<List<Office>> GetAsync();
    Task<Office?> GetAsync(Guid id);
    Task<Office> CreateAsync(OfficeViewModel vm);
    Task<Office> UpdateAsync(Office office);
    Task RemoveAsync(Guid id);
}

public class OfficeService : IOfficeService
{
    private readonly IMongoCollection<Office> _officesCollection;
    private readonly Mapper _mapper;

    public OfficeService(IOptions<OfficesDatabaseSettings> settings, IMongoClient mongoClient)
    {
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _officesCollection = mongoDatabase.GetCollection<Office>(settings.Value.OfficesCollection);
        _mapper = new Mapper(new MapperConfiguration(cfg => cfg.CreateMap<OfficeViewModel, Office>()));
    }

    public async Task<List<Office>> GetAsync()
    {
        using var cursor =  await _officesCollection.FindAsync(new BsonDocument());
        return await cursor.ToListAsync();
    }

    public async Task<Office?> GetAsync(Guid id)
    {
        using var cursor = await _officesCollection.FindAsync(x => x.Id == id);
        return await cursor.FirstOrDefaultAsync();
    }
    
    public async Task<Office> CreateAsync(OfficeViewModel vm)
    {
        var office = _mapper.Map<Office>(vm);
        office.Id = Guid.NewGuid();
        await _officesCollection.InsertOneAsync(office);
        return office;
    }

    public async Task<Office> UpdateAsync(Office office)
    {
        var result = await _officesCollection.ReplaceOneAsync(x => x.Id == office.Id, office);

        if (result.IsAcknowledged)
            return office;
         
        throw new ArgumentException("Unable to update the entity");
    }

    public async Task RemoveAsync(Guid id)
    {
        var result = await _officesCollection.DeleteOneAsync(x => x.Id == id);

        if (!result.IsAcknowledged)
            throw new ArgumentException("Unable to delete the entity");
    }
}

