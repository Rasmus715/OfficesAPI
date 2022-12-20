using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OfficesAPI.Services;
using OfficesAPI.ViewModels;
using Office = OfficesAPI.Models.Office;

namespace OfficesAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OfficesController : ControllerBase
{
    private readonly IOfficeService _officeService;
    private readonly IValidator<OfficeViewModel> _officeViewModelValidator;
    private readonly IValidator<Office> _officeValidator;

    public OfficesController(IOfficeService officeService, IValidator<OfficeViewModel> validator, IValidator<Office> officeValidator)
    {
        _officeService = officeService;
        _officeViewModelValidator = validator;
        _officeValidator = officeValidator;
    }

    [HttpGet]
    [Route("GetAll")]
    public async Task<List<Office>> GetAll()
    {
        return await _officeService.GetAsync();
    }
    
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid id)
    {
        //Console.WriteLine(JsonSerializer.Serialize(Request.Body));
        var office = await _officeService.GetAsync(id);

        if (office is null)
        {
            return NotFound();
        }
        
        return Ok(office);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(OfficeViewModel vm)
    {
        var validateResult = await _officeViewModelValidator.ValidateAsync(vm);
        if (!validateResult.IsValid)
        {
            return UnprocessableEntity(new {errors = validateResult.Errors.Select(i=>i.ErrorMessage)});
        }
        
        var result = await _officeService.CreateAsync(vm);

        return CreatedAtAction(nameof(Create), new { id = result.Id }, result);
    }
    
    [HttpPatch]
    public async Task<IActionResult> Update(Office newOffice)
    {

        await LogBody();
        var validateResult = await _officeValidator.ValidateAsync(newOffice);
        if (!validateResult.IsValid)
        {
            return UnprocessableEntity(new { errors = validateResult.Errors.Select(i=>i.ErrorMessage)});
        }
        
        var office = await _officeService.GetAsync(newOffice.Id);

        if (office is null)
        {
            return NotFound();
        }

        await _officeService.UpdateAsync(newOffice);
        
        return Ok();
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

    private async Task LogBody()
    {
        HttpContext.Request.EnableBuffering();

        var requestBodyStream = new MemoryStream();
        var originalRequestBody = HttpContext.Request.Body;

        await HttpContext.Request.Body.CopyToAsync(requestBodyStream);
        requestBodyStream.Seek(0, SeekOrigin.Begin);
        
        var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();

        requestBodyStream.Seek(0, SeekOrigin.Begin);
        HttpContext.Request.Body = requestBodyStream;
        
        HttpContext.Request.Body = originalRequestBody;

        Console.WriteLine(requestBodyText);
    }
}