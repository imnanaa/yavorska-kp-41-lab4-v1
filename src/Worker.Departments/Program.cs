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

channel.ExchangeDeclare("topic_logs", ExchangeType.Topic);

void Create(string queue, string key)
{
    channel.QueueDeclare(
        queue: queue,
        durable: false,
        exclusive: false,
        autoDelete: false
    );

    channel.QueueBind(queue, "topic_logs", key);

    var consumer = new EventingBasicConsumer(channel);

    consumer.Received += (m, ea) =>
    {
        try
        {
            var json = Encoding.UTF8.GetString(ea.Body.ToArray());

            if (string.IsNullOrWhiteSpace(json))
                throw new Exception("Empty message");

            var msg = JsonSerializer.Deserialize<Message>(json);

            if (msg == null)
                throw new Exception("Null message");

            Console.WriteLine($"=== {queue} ===");
            Console.WriteLine($"ID: {msg.Id}");
            Console.WriteLine($"Time: {msg.Timestamp}");
            Console.WriteLine($"Content: {msg.Content}");
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"[{queue}] JSON Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{queue}] Error: {ex.Message}");
        }
    };

    channel.BasicConsume(queue, true, consumer);
}

Create("marketing_queue", "#.company.marketing");
Create("sales_queue", "#.company.sales");
Create("engineering_queue", "#.company.engineering");

Console.ReadLine();