using System.Text.Json;
using CommunicationModels;
using Microsoft.IO;
using OfficesAPI.RabbitMq;

namespace OfficesAPI.Extensions;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRabbitMqService _rabbitMqService;
    private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;
    public RequestResponseLoggingMiddleware(RequestDelegate next, 
        IRabbitMqService rabbitMqService)
    {
        _next = next;
        _rabbitMqService = rabbitMqService;
        _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
    }
    
    public async Task Invoke(HttpContext context)
    {
        await LogRequest(context);
        await _next(context);
    }

    private async Task LogRequest(HttpContext context)
    {
        context.Request.EnableBuffering();
        await using var requestStream = _recyclableMemoryStreamManager.GetStream();
        await context.Request.Body.CopyToAsync(requestStream);
        var obj = new RabbitLog
        {
            Id = Guid.NewGuid().ToString(),
            Date = DateTime.Now,
            ServiceName = "OfficesAPI",
            MethodName = $"{context.Request.Method} {context.Request.Path}" ,
            MethodBody = JsonSerializer.Serialize(new
            {
                QueryStrings = context.Request.QueryString.Value, 
                Body = ReadStreamInChunks(requestStream)
            })
        };
        Console.WriteLine(JsonSerializer.Serialize(obj));
        _rabbitMqService.SendMessage(obj);
        context.Request.Body.Position = 0;
    }
    
    
    private static string ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream);
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        do
        {
            readChunkLength = reader.ReadBlock(readChunk, 
                0, 
                readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);
        return textWriter.ToString();
    }
}