using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var host = config["RabbitMQ:Host"] ?? "rabbitmq";

var factory = new ConnectionFactory()
{
    HostName = host
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("direct_logs", ExchangeType.Direct);

channel.QueueDeclare(
    queue: "dev_queue",
    durable: false,
    exclusive: false,
    autoDelete: false
);

channel.QueueBind("dev_queue", "direct_logs", "dev");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception("Empty message");

        var msg = JsonSerializer.Deserialize<Message>(json);

        if (msg == null)
            throw new Exception("Null message");

        Console.WriteLine("=== DEV MESSAGE ===");
        Console.WriteLine($"ID: {msg.Id}");
        Console.WriteLine($"Time: {msg.Timestamp}");
        Console.WriteLine($"Content: {msg.Content}");
    }
    catch (JsonException ex)
    {
        Console.WriteLine($"JSON Error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }
};

channel.BasicConsume("dev_queue", true, consumer);

Console.ReadLine();