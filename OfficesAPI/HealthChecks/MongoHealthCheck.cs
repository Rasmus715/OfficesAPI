using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using OfficesAPI.Models;

namespace OfficesAPI.HealthChecks;

public class MongoHealthCheck : IHealthCheck
{
    private IMongoDatabase _db { get; set; }
    private MongoClient _mongoClient { get; set; }
 
    public MongoHealthCheck(IOptions<OfficesDatabaseSettings> officesDatabaseSettings)
    {
        _mongoClient = new MongoClient(officesDatabaseSettings.Value.ConnectionString);
 
        _db = _mongoClient.GetDatabase(officesDatabaseSettings.Value.DatabaseName);
 
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new())
    {
        var healthCheckResultHealthy = await CheckMongoDbConnectionAsync();
        
        return healthCheckResultHealthy ? 
            HealthCheckResult.Healthy("MongoDB health check success") : 
            HealthCheckResult.Unhealthy("MongoDB health check failure");

        ;
    }
    
    private async Task<bool> CheckMongoDbConnectionAsync()
    {
        try
        {
            await _db.RunCommandAsync((Command<BsonDocument>)"{ping:1}");
        }
 
        catch (Exception)
        {
            return false;
        }
 
        return true;
    }
}