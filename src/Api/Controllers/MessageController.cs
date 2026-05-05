using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shared;

namespace Api.Controllers;

[ApiController]
[Route("api/messages")]
public class MessageController : ControllerBase
{
    private readonly RabbitSettings _settings;

    public MessageController(IOptions<RabbitSettings> settings)
    {
        _settings = settings.Value;
    }

    private IConnection GetConnection()
    {
        var factory = new ConnectionFactory() { HostName = _settings.Host };
        return factory.CreateConnection();
    }

    // ===== DIRECT (REST style) =====

    [HttpPost("direct/dev")]
    public IActionResult SendToDev([FromBody] MessageRequest request)
    {
        return SendDirectMessage("dev", request.Content);
    }

    [HttpPost("direct/prod")]
    public IActionResult SendToProd([FromBody] MessageRequest request)
    {
        return SendDirectMessage("prod", request.Content);
    }

    private IActionResult SendDirectMessage(string routingKey, string content)
    {
        try
        {
            using var connection = GetConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(_settings.DirectExchange, ExchangeType.Direct);

            var message = new Message { Content = content };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(_settings.DirectExchange, routingKey, null, body);

            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    // ===== TOPIC (REST style) =====

    [HttpPost("topic/marketing")]
    public IActionResult SendToMarketing([FromBody] MessageRequest request)
    {
        return SendTopicMessage("marketing", request.Content);
    }

    [HttpPost("topic/sales")]
    public IActionResult SendToSales([FromBody] MessageRequest request)
    {
        return SendTopicMessage("sales", request.Content);
    }

    [HttpPost("topic/engineering")]
    public IActionResult SendToEngineering([FromBody] MessageRequest request)
    {
        return SendTopicMessage("engineering", request.Content);
    }

    private IActionResult SendTopicMessage(string type, string content)
    {
        try
        {
            using var connection = GetConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(_settings.TopicExchange, ExchangeType.Topic);

            var routingKey = $"app.company.{type}";

            var message = new Message { Content = content };
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            channel.BasicPublish(_settings.TopicExchange, routingKey, null, body);

            return Ok(message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}