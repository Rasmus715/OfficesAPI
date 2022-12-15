using System.Diagnostics;
using System.Reflection;
using AutoMapper;
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
    Task<Office> UpdateAsync(Guid id, OfficeViewModel updatedOffice);
    Task RemoveAsync(Guid id);
}

public class OfficeService : IOfficeService
{
    private readonly IMongoCollection<Office> _officesCollection;
    private readonly Mapper _mapper;

    public OfficeService(AppDbContext dbContext)
    {
        _officesCollection = dbContext.GetCollection();
        var config = new MapperConfiguration(cfg => cfg.CreateMap<OfficeViewModel, Office>());
        _mapper = new Mapper(config);
    }

    public async Task<List<Office>> GetAsync()
    {
        return await _officesCollection.Find(_ => true).ToListAsync();
    }

    public async Task<Office?> GetAsync(Guid id)
    {
        return await _officesCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
    }
    
    public async Task<Office> CreateAsync(OfficeViewModel vm)
    {
        var office = _mapper.Map<Office>(vm);
        office.Id = Guid.NewGuid();
        await _officesCollection.InsertOneAsync(office);
        return office;
    }

    public async Task<Office> UpdateAsync(Guid id, OfficeViewModel updatedOffice)
    {
        var newOffice = _mapper.Map<Office>(updatedOffice);
        newOffice.Id = id;
        await _officesCollection.ReplaceOneAsync(x => x.Id == id, newOffice);
        return newOffice;
    }

    public async Task RemoveAsync(Guid id)
    {
        await _officesCollection.DeleteOneAsync(x => x.Id == id);
    }
}

