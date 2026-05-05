var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RabbitSettings>(
    builder.Configuration.GetSection("RabbitMQ"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();

public class RabbitSettings
{
    public string Host { get; set; }
    public string DirectExchange { get; set; }
    public string TopicExchange { get; set; }
}