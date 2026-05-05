using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Shared;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var host = config["RabbitMQ:Host"];
var factory = new ConnectionFactory()
{
    HostName = host
};


using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("direct_logs", ExchangeType.Direct);
channel.QueueDeclare("prod_queue");

channel.QueueBind("prod_queue", "direct_logs", "prod");

var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    try
    {
        var json = Encoding.UTF8.GetString(ea.Body.ToArray());

        if (string.IsNullOrWhiteSpace(json))
            throw new Exception("Empty message received");

        var msg = JsonSerializer.Deserialize<Message>(json);

        if (msg == null)
            throw new Exception("Deserialization returned null");

        Console.WriteLine("=== PROD MESSAGE ===");
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
        Console.WriteLine($"General Error: {ex.Message}");
    }
};

channel.BasicConsume("prod_queue", true, consumer);

Console.ReadLine();