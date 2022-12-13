namespace OfficesAPI.Models;

public class OfficesDatabaseSettings
{
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public string OfficesCollection { get; set; } = null!;
}