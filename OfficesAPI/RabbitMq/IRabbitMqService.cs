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
        
        var body = Encoding.UTF8.GetBytes(message);
        
        var properties = channel.CreateBasicProperties();
        
        properties.Headers = new Dictionary<string, object>
        {
            {
                "type", "logs"
            }
        };
        
        properties.Expiration = "10000";
        
        channel.BasicPublish(
            "LogsExchangeHeaders",
            string.Empty,
            properties,
            body);
    }
}