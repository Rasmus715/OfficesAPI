using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OfficesAPI.Models;
using OfficesAPI.Services;
using OfficesAPI.ViewModels;

namespace OfficesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfficesController : ControllerBase
{
    private readonly IOfficeService _officeService;
    private readonly IValidator<OfficeViewModel> _officeViewModelValidator;

    public OfficesController(IOfficeService officeService, IValidator<OfficeViewModel> validator)
    {
        _officeService = officeService;
        _officeViewModelValidator = validator;
    }

    [HttpGet]
    public async Task<List<Office>> Get()
    {
        return await _officeService.GetAsync();
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Office>> Get(Guid id)
    {
        var office = await _officeService.GetAsync(id);

        if (office is null)
        {
            return NotFound();
        }

        return office;
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(OfficeViewModel vm)
    {
        var validateResult = await _officeViewModelValidator.ValidateAsync(vm);
        if (!validateResult.IsValid)
        {
            return UnprocessableEntity(new {errors=validateResult.Errors.Select(i=>i.ErrorMessage)});
        }
        var result = await _officeService.CreateAsync(vm);

        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    
    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(Guid id , OfficeViewModel vm)
    {
        var validateResult = await _officeViewModelValidator.ValidateAsync(vm);
        if (!validateResult.IsValid)
        {
            return UnprocessableEntity(new {errors=validateResult.Errors.Select(i=>i.ErrorMessage)});
        }
        
        var book = await _officeService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        return Ok(await _officeService.UpdateAsync(id, vm));
    }
    
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var book = await _officeService.GetAsync(id);

        if (book is null)
        {
            return NotFound();
        }

        await _officeService.RemoveAsync(id);

        return NoContent();
    }
}