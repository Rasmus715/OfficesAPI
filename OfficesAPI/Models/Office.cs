using FluentValidation;
using MongoDB.Bson.Serialization.Attributes;

namespace OfficesAPI.Models;

public class Office
{
    [BsonId]
    public Guid Id { get; set; }
    public string Address { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string RegistryPhoneNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class OfficeValidator : AbstractValidator<Office> 
{
    public OfficeValidator() 
    {
        RuleFor(x => x.Id).NotNull();
        RuleFor(x => x.Address).NotNull();
        RuleFor(x => x.RegistryPhoneNumber).NotNull();
    }
}