using EasyNetQ;
using Messages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Sample01
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                {
                    services.AddHostedService<ProducerAndConsumer>();
                });
    }
}

public class ProducerAndConsumer : BackgroundService
{
    private readonly IBus _bus;

    public ProducerAndConsumer()
    {
        _bus = RabbitHutch.CreateBus("host=localhost");
        _bus.PubSub.Subscribe<TextMessage>("consumer", HandleTextMessage);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await _bus.PubSub.PublishAsync(new TextMessage { Text = Guid.NewGuid().ToString() }, cancellationToken);
            Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] Message published!");
            await Task.Delay(1000, cancellationToken);
        }
    }

    static void HandleTextMessage(TextMessage textMessage)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"[{DateTime.Now:hh:mm:ss}] Got message: {textMessage.Text}");
        Console.ResetColor();
    }
}

namespace Messages
{
    public class TextMessage
    {
        public string Text { get; set; }
    }
}