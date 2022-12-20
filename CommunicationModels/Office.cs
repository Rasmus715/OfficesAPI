namespace CommunicationModels;

public class Office
{
    public Guid Id { get; set; }
    public string Address { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string RegistryPhoneNumber { get; set; } = null!; 
    public bool IsActive { get; set; }
}