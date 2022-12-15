using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace OfficesAPI.RabbitMq;

public interface IRabbitMqService
{
    void SendMessage(object obj);
}

public class RabbitMqService : IRabbitMqService
{
    public void SendMessage(object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        
        var factory = new ConnectionFactory
        {
            HostName = "localhost"
        };
        
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        
        channel.QueueDeclare(
            "LogsQueue",
            false,
            false,
            false,
            null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            "",
            "LogsQueue",
            null,
            body);
    }
}