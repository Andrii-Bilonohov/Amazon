using RabbitMQ.Client;

namespace AmazonCopyDb.App;

public static class RabbitMqCheck
{
  public static async Task RunAsync() 
  {
    var factory = new ConnectionFactory
    {
      HostName = "127.0.0.1",
      Port = 5672,
      UserName = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_USER"),
      Password = Environment.GetEnvironmentVariable("RABBITMQ_DEFAULT_PASS")
    };
    await using var connection = await factory.CreateConnectionAsync();
    await using var channel = await connection.CreateChannelAsync();
    await channel.QueueDeclareAsync("amazon.test", durable: true, exclusive: false, autoDelete: false);
    Console.WriteLine("RabbitMQ OK");
 }
}
