using System.Text.Json;
using CommunicationModels;
using FluentValidation;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using OfficesAPI.RabbitMq;
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
    private readonly IRabbitMqService _rabbitMqService;

    public OfficesController(IOfficeService officeService, IValidator<OfficeViewModel> validator, IRabbitMqService rabbitMqService)
    {
        _officeService = officeService;
        _officeViewModelValidator = validator;
        _rabbitMqService = rabbitMqService;
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
    public async Task<IActionResult> Update([FromQuery]Guid id , [FromBody]OfficeViewModel vm)
    {

        await LogBody();
        var validateResult = await _officeViewModelValidator.ValidateAsync(vm);
        if (!validateResult.IsValid)
        {
            return UnprocessableEntity(new { errors = validateResult.Errors.Select(i=>i.ErrorMessage)});
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
    private async Task Log(HttpRequest request)
    {
        var req2 = request;
        var req = request.Body;
        var json = JsonSerializer.Serialize(request);

        _rabbitMqService.SendMessage(new RabbitLog
        {
            Id = Guid.NewGuid().ToString(),
            Date = DateTime.Now,
            ServiceName = "OfficesAPI",
            MethodName = GetControllerData(),
            MethodBody = json
        });
    }
    private string GetControllerData()
    {
        return $"{ControllerContext.RouteData.Values["controller"]}/{ControllerContext.RouteData.Values["action"]}";
    }
}