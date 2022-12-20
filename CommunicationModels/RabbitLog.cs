namespace CommunicationModels;

public class RabbitLog
{
    public string Id { get; set; } = null!;
    public DateTime Date { get; set; }
    public string ServiceName { get; set; } = null!;
    public string MethodName { get; set; } = null!;
    public string? MethodBody { get; set; }
}