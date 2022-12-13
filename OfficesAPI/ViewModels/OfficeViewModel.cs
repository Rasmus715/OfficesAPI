using FluentValidation;

namespace OfficesAPI.ViewModels;

public class OfficeViewModel
{
    public string Address { get; set; } = null!;
    public Guid? PhotoId { get; set; }
    public string RegistryPhoneNumber { get; set; } = null!;
    public bool IsActive { get; set; }
}

public class OfficeViewModelValidator : AbstractValidator<OfficeViewModel> 
{
    public OfficeViewModelValidator()
    {
        RuleFor(x => x.Address).NotNull().MaximumLength(70);
        RuleFor(x => x.RegistryPhoneNumber).NotNull();
    }
}