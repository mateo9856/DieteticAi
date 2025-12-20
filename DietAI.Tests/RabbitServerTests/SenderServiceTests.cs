using System.Text;
using DietAI.RabbitServer.Abstractions.RabbitConnection;
using DietAI.RabbitServer.Implementations.SenderService;
using NSubstitute;
using RabbitMQ.Client;

namespace DietAI.Tests.RabbitServerTests;

public class SenderServiceTests
{
    private ITopicFactory _topicFactory = null!;
    private IChannel _channel = null!;
    private SenderService _sender = null!;

    [SetUp]
    public void Setup()
    {
        _topicFactory = Substitute.For<ITopicFactory>();
        _channel = Substitute.For<IChannel>();
        _topicFactory.ActiveChannel.Returns(_channel);

        _sender = new SenderService(_topicFactory);
    }

    [Test]
    public async Task SendToQueueAsync_String_Publishes_To_DefaultExchange()
    {
        var queue = "test-queue";
        var message = "hello";

        await _sender.SendToQueueAsync(queue, message, persistent: false);

        var calls = _channel.ReceivedCalls();
        Assert.That(calls.Any(c => c.GetMethodInfo().Name == "BasicPublishAsync"));
    }

    [Test]
    public async Task SendToQueueAsync_ByteArray_Publishes_RawBytes()
    {
        var queue = "q";
        var payload = new byte[] { 1, 2, 3 };

        await _sender.SendToQueueAsync(queue, payload, persistent: true);

        var calls = _channel.ReceivedCalls();
        Assert.That(calls.Any(c => c.GetMethodInfo().Name == "BasicPublishAsync"));
    }

    [Test]
    public async Task SendToQueueAsync_Generic_Serializes_And_Publishes()
    {
        var queue = "qgen";
        var obj = new { X = 5 };

        await _sender.SendToQueueAsync(queue, obj, persistent: false);

        // ensure BasicPublishAsync was called and the payload contains serialized JSON
        var calls = _channel.ReceivedCalls().Where(c => c.GetMethodInfo().Name == "BasicPublishAsync");
        Assert.That(calls.Any());
        var found = false;
        foreach (var c in calls)
        {
            var args = c.GetArguments();
            foreach (var a in args)
            {
                if (a is byte[] b)
                {
                    var s = Encoding.UTF8.GetString(b);
                    if (s.Contains("\"X\":5")) found = true;
                }
                else if (a is ReadOnlyMemory<byte> rom)
                {
                    var s = Encoding.UTF8.GetString(rom.ToArray());
                    if (s.Contains("\"X\":5")) found = true;
                }
                else if (a is Memory<byte> mem)
                {
                    var s = Encoding.UTF8.GetString(mem.ToArray());
                    if (s.Contains("\"X\":5")) found = true;
                }
                else if (a is ArraySegment<byte> seg)
                {
                    var s = Encoding.UTF8.GetString(seg.ToArray());
                    if (s.Contains("\"X\":5")) found = true;
                }
            }
        }
        Assert.That(found, Is.True);
    }

    [Test]
    public async Task PublishToExchangeAsync_Uses_ProvidedExchange_And_RoutingKey()
    {
        var exchange = "ex";
        var routing = "rk";
        var message = "m";

        await _sender.PublishToExchangeAsync(exchange, routing, message, persistent: true);

        var calls = _channel.ReceivedCalls();
        Assert.That(calls.Any(c => c.GetMethodInfo().Name == "BasicPublishAsync"));
    }
}
