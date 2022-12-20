using FluentValidation;
using MassTransit;
using MongoDB.Driver;
using OfficesAPI;
using OfficesAPI.Data;
using OfficesAPI.Extensions;
using OfficesAPI.HealthChecks;
using OfficesAPI.Models;
using OfficesAPI.RabbitMq;
using OfficesAPI.Services;
using OfficesAPI.ViewModels;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Logging.ConfigureLogging();

builder.Services.Configure<OfficesDatabaseSettings>(
    builder.Configuration.GetSection("OfficesDatabase"));

// Add services to the container.

builder.Host.UseSerilog();
builder.Services.AddControllers();
builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("MongoDBConnectionCheck");
builder.Services.AddCors(options => {
    options.AddPolicy("CORSPolicy", corsPolicyBuilder => corsPolicyBuilder
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true));
});
builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<IOfficeService, OfficeService>();
builder.Services.AddScoped<IValidator<OfficeViewModel>, OfficeViewModelValidator>();
builder.Services.AddScoped<IValidator<Office>, OfficeValidator>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(builder.Configuration.GetValue<string>("OfficesDatabase:ConnectionString")));
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context,cfg) =>
    {
        cfg.Host("localhost", "/");
        cfg.ConfigureEndpoints(context);
    });
});



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
app.MapHealthChecks("/mongoHealthz");

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<RequestResponseLoggingMiddleware>();
    
app.UseCors("CORSPolicy");
app.MapControllers();

app.Run();