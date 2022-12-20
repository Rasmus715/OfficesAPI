using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Moq;
using OfficesAPI.Data;
using OfficesAPI.Models;
using OfficesAPI.Services;
using OfficesAPI.ViewModels;

namespace OfficesAPI.Tests;

[TestClass]
public class OfficeServiceTests
{
    private readonly Mock<IOptions<OfficesDatabaseSettings>> _mockOptions;
    private readonly Mock<IMongoDatabase> _mockDatabase;
    private readonly Mock<IMongoClient> _mockClient;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly Mock<IMongoCollection<Office>> _mockCollection = new();


    public OfficeServiceTests()
    {
        _mockOptions = new Mock<IOptions<OfficesDatabaseSettings>>();
        _mockDatabase = new Mock<IMongoDatabase>();
        _mockClient = new Mock<IMongoClient>();
        _mockDbContext = new Mock<AppDbContext>();
        _mockDbContext.Setup(s => s.GetCollection()).Returns(_mockCollection.Object);
        
        var settings = new OfficesDatabaseSettings
        {
            ConnectionString = "mongodb://tes123",
            DatabaseName = "databaseName",
            OfficesCollection = "officesCollection"
        };
 
        _mockOptions.Setup(s => s.Value).Returns(settings);
        
        _mockClient.Setup(c => c
                .GetDatabase(_mockOptions.Object.Value.DatabaseName, null))
            .Returns(_mockDatabase.Object);
        
        _mockClient.Setup(c => c.GetDatabase(
            It.IsAny<string>(),
            It.IsAny<MongoDatabaseSettings>()
        )).Returns(_mockDatabase.Object);

        _mockDatabase.Setup(c => c.GetCollection<Office>(
            It.IsAny<string>(),
            It.IsAny<MongoCollectionSettings>()
        )).Returns(_mockCollection.Object);
    }
    
    
    [TestMethod]
    public async Task GetAsync()
    {
        //Arrange
        var cursor = new Mock<IAsyncCursor<Office>?>();
        _mockClient.Setup(c => c.GetDatabase(
            It.IsAny<string>(),
            It.IsAny<MongoDatabaseSettings>()
        )).Returns(_mockDatabase.Object);

        _mockDatabase.Setup(c => c.GetCollection<Office>(
            It.IsAny<string>(),
            It.IsAny<MongoCollectionSettings>()
        )).Returns(_mockCollection.Object);
        
        _mockCollection
            .Setup(_ => _.FindAsync(
                It.IsAny<FilterDefinition<Office>>(), 
                It.IsAny<FindOptions<Office, Office>>(), 
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(cursor.Object);
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        //Act
        var result = await officeService.GetAsync();

        //Assert 
        result.Should().BeOfType(typeof(List<Office>));
    }

    [TestMethod]
    public async Task GetByIdAsync()
    {
        //Arrange
        var guid = Guid.NewGuid();
        
        var list = new List<Office>
        {
            new()
            {
                Id = guid
            }
        };
        
        var cursor = new Mock<IAsyncCursor<Office>?>();
        cursor.Setup(_ => _!.Current).Returns(list);
        
        //https://stackoverflow.com/questions/48176063/mongodb-c-sharp-driver-mock-method-that-returns-iasynccursor
        //I solemnly swear that I don't know how this thing works
        cursor
            .SetupSequence(_ => _.MoveNext(It.IsAny<CancellationToken>()))
            .Returns(true)
            .Returns(false);
        cursor
            .SetupSequence(_ => _.MoveNextAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(true))
            .Returns(Task.FromResult(false));

        _mockCollection
            .Setup(_ => _.FindAsync(
                It.IsAny<FilterDefinition<Office>>(), 
                It.IsAny<FindOptions<Office, Office>>(), 
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(cursor.Object);
        
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        //Act
        var result = await officeService.GetAsync(guid);

        //Assert 
        result.Should().BeOfType(typeof(Office));
    }

    [TestMethod]
    public async Task CreateAsync()
    {
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        var result = await officeService.CreateAsync(new OfficeViewModel());
        
        result.Should().BeOfType(typeof(Office));
    }
    
    [TestMethod]
    public async Task UpdateAsyncThrowsArgumentException()
    {
        //Arrange
        var officeToUpdate = new Office
        {
            Id = Guid.NewGuid()
        };
        
        var mockReplaceResult = new Mock<ReplaceOneResult>();
        
        _mockCollection
            .Setup(_ => _.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Office>>(), 
                It.IsAny<Office>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockReplaceResult.Object);
        
        
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        Func<Task<Office>> act = async () => await officeService.UpdateAsync(officeToUpdate);

        //Act, Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [TestMethod]
    public async Task UpdateAsync()
    {
        //Arrange
        var officeToUpdate = new Office
        {
            Id = Guid.NewGuid()
        };
        
        var mockReplaceResult = new Mock<ReplaceOneResult>();

        mockReplaceResult.Setup(c => c.IsAcknowledged).Returns(true);
        
        _mockCollection
            .Setup(_ => _.ReplaceOneAsync(
                It.IsAny<FilterDefinition<Office>>(), 
                It.IsAny<Office>(),
                It.IsAny<ReplaceOptions>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockReplaceResult.Object);

        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        //Act
        var result = await officeService.UpdateAsync(officeToUpdate);
        
        //Assert
        result.Should().BeOfType(typeof(Office));
    }

    [TestMethod]
    public async Task DeleteAsync()
    {
        //Arrange
        var mockDeleteResult = new Mock<DeleteResult>();

        mockDeleteResult.Setup(c => c.IsAcknowledged).Returns(true);

        _mockCollection
            .Setup(_ => _.DeleteOneAsync(
                It.IsAny<FilterDefinition<Office>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockDeleteResult.Object);
        
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        
        //Act
        await officeService.RemoveAsync(Guid.NewGuid());
        
        //Assert
        true.Should().BeTrue();
    }
    
    [TestMethod]
    public async Task DeleteAsyncThrowsArgumentException()
    {
        //Arrange
        var mockDeleteResult = new Mock<DeleteResult>();
        
        _mockCollection
            .Setup(_ => _.DeleteOneAsync(
                It.IsAny<FilterDefinition<Office>>(),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(mockDeleteResult.Object);
        
        var officeService = new OfficeService(_mockOptions.Object, _mockClient.Object);
        Func<Task> act = async () => await officeService.RemoveAsync(Guid.NewGuid());
        
        //Act, assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}