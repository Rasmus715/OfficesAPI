using FluentValidation;
using OfficesAPI.Data;
using OfficesAPI.Models;
using OfficesAPI.Services;
using OfficesAPI.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OfficesDatabaseSettings>(
    builder.Configuration.GetSection("OfficesDatabase"));

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IValidator<OfficeViewModel>, OfficeViewModelValidator>();
builder.Services.AddScoped<IValidator<Office>, OfficeValidator>();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();